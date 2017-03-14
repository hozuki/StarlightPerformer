using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using StarlightPerformer.Beatmap.Parsing;

namespace StarlightPerformer.Beatmap {
    public sealed class Score {

        public static Score FromString(string csv, Encoding encoding) {
            var bytes = encoding.GetBytes(csv);
            using (var memoryStream = new MemoryStream(bytes, false)) {
                return FromStream(memoryStream, encoding);
            }
        }

        public static Score FromString(string csv) => FromString(csv, Encoding.UTF8);

        public static Score FromStream(Stream stream) => FromStream(stream, Encoding.UTF8);

        public static Score FromStream(Stream stream, Encoding encoding) {
            using (var reader = new StreamReader(stream, encoding)) {
                var config = new CsvConfiguration();
                config.RegisterClassMap<ScoreCsvMap>();
                config.HasHeaderRecord = true;
                using (var csv = new CsvReader(reader, config)) {
                    var score = new Score();
                    var items = score._notes;

                    while (csv.Read()) {
                        var note = csv.GetRecord<Note>();
                        items.Add(note);
                    }
                    items.Sort((s1, s2) => s1.HitTiming > s2.HitTiming ? 1 : (s2.HitTiming > s1.HitTiming ? -1 : 0));
                    score.Resolve();

                    return score;
                }
            }
        }

        public static Score FromFile(string fileName) => FromFile(fileName, Encoding.UTF8);

        public static Score FromFile(string fileName, Encoding encoding) {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                return FromStream(fileStream, encoding);
            }
        }

        public static bool IsScoreFile(string fileName, out string[] supportedNames) {
            supportedNames = null;
            var connectionString = $"Data Source={SanitizeString(fileName)};";
            using (var connection = new SQLiteConnection(connectionString)) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT name FROM blobs WHERE name LIKE 'musicscores/m___/%.csv';";
                    try {
                        using (var reader = command.ExecuteReader()) {
                            var names = new List<string>();
                            var result = false;
                            while (reader.Read()) {
                                names.Add(reader.GetString(0));
                                result = true;
                            }
                            supportedNames = names.ToArray();
                            if (result) {
                                connection.Close();
                                return true;
                            }
                        }
                    } catch (Exception ex) when (ex is SQLiteException || ex is InvalidOperationException) {
                        connection.Close();
                        return false;
                    }
                }
                connection.Close();
            }
            return false;
        }

        public static bool ContainsDifficulty(string[] names, Difficulty difficulty) {
            if (difficulty == Difficulty.Invalid) {
                throw new IndexOutOfRangeException("Invalid difficulty.");
            }
            var n = (int)difficulty;
            if (n > names.Length) {
                return false;
            }
            return DifficultyRegexes[n - 1].IsMatch(names[n - 1]);
        }

        public int[] Validate() {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Note> Notes => _notes;

        private void Resolve() {
            var notes = _notes;
            var holdNotesToBeMatched = new List<Note>();
            var flickGroupNoteCount = new Dictionary<int, int>();
            var slideGroupNoteCount = new Dictionary<int, int>();
            var i = 0;
            foreach (var note in notes) {
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                        if (note.IsSync) {
                            var syncPairItem = notes.FirstOrDefault(n => n != note && n.HitTiming.Equals(note.HitTiming) && n.IsSync);
                            if (syncPairItem == null) {
                                throw new FormatException($"Missing sync pair note at note ID #{note.ID}.");
                            }
                            note.SyncPair = syncPairItem;
                        }
                        if (note.IsFlick) {
                            if (!flickGroupNoteCount.ContainsKey(note.GroupID)) {
                                flickGroupNoteCount.Add(note.GroupID, 0);
                            }
                            ++flickGroupNoteCount[note.GroupID];
                            var nextFlickItem = notes.Skip(i + 1).FirstOrDefault(n => (n.IsFlick || n.IsSlide) && n.GroupID != 0 && n.GroupID == note.GroupID);
                            if (nextFlickItem == null) {
                                if (flickGroupNoteCount[note.GroupID] < 2) {
                                    Debug.WriteLine($"[WARNING] No enough flick notes to form a flick group at note ID #{note.ID}, group ID {note.GroupID}.");
                                }
                            } else {
                                note.NextFlickOrSlide = nextFlickItem;
                                nextFlickItem.PrevFlickOrSlide = note;
                                if (nextFlickItem.IsSlide) {
                                    note.NextFlickOrSlide = nextFlickItem;
                                    nextFlickItem.PrevFlickOrSlide = note;
                                }
                            }
                        }
                        break;
                    case NoteType.Hold:
                        if (note.IsSync) {
                            var syncPairItem = notes.FirstOrDefault(n => n != note && n.HitTiming.Equals(note.HitTiming) && n.IsSync);
                            if (syncPairItem == null) {
                                throw new FormatException($"Missing sync pair note at note ID #{note.ID}.");
                            }
                            note.SyncPair = syncPairItem;
                        }
                        if (holdNotesToBeMatched.Contains(note)) {
                            holdNotesToBeMatched.Remove(note);
                            break;
                        }
                        var endHoldItem = notes.Skip(i + 1).FirstOrDefault(n => n.FinishPosition == note.FinishPosition);
                        if (endHoldItem == null) {
                            throw new FormatException($"Missing end hold note at note ID #{note.ID}.");
                        }
                        note.HoldPair = endHoldItem;
                        endHoldItem.HoldPair = note;
                        // The end hold note always follows the trail of start hold note, so the literal value of its 'start' field is ignored.
                        // See song_1001, 'Master' difficulty, #189-#192, #479-#483.
                        endHoldItem.StartPosition = note.StartPosition;
                        holdNotesToBeMatched.Add(endHoldItem);
                        break;
                    case NoteType.Slide:
                        if (!slideGroupNoteCount.ContainsKey(note.GroupID)) {
                            slideGroupNoteCount.Add(note.GroupID, 0);
                        }
                        if (note.IsSync) {
                            var syncPairItem = notes.FirstOrDefault(n => n != note && n.HitTiming.Equals(note.HitTiming) && n.IsSync);
                            if (syncPairItem == null) {
                                throw new FormatException($"Missing sync pair note at note ID #{note.ID}.");
                            }
                            note.SyncPair = syncPairItem;
                        }
                        if (holdNotesToBeMatched.Contains(note)) {
                            holdNotesToBeMatched.Remove(note);
                            break;
                        }
                        var nextSlideItem = notes.Skip(i + 1).FirstOrDefault(n => (n.IsSlide || n.IsFlick) && n.GroupID == note.GroupID);
                        if (nextSlideItem == null) {
                            if (slideGroupNoteCount[note.GroupID] < 2) {
                                Debug.WriteLine($"[WARNING] No enough slide notes to form a slide group at note ID #{note.ID}, group ID {note.GroupID}.");
                            }
                        } else {
                            note.NextFlickOrSlide = nextSlideItem;
                            nextSlideItem.PrevFlickOrSlide = note;
                        }
                        break;
                }
                ++i;
            }
        }

        private static string SanitizeString(string s) {
            var shouldCoverWithQuotes = false;
            if (s.IndexOf('"') >= 0) {
                s = s.Replace("\"", "\"\"\"");
                shouldCoverWithQuotes = true;
            }
            if (s.IndexOfAny(CommandlineEscapeChars) >= 0) {
                shouldCoverWithQuotes = true;
            }
            if (s.Any(c => c > 127)) {
                shouldCoverWithQuotes = true;
            }
            return shouldCoverWithQuotes ? "\"" + s + "\"" : s;
        }

        private readonly List<Note> _notes = new List<Note>();

        private static readonly char[] CommandlineEscapeChars = { ' ', '&', '%', '#', '@', '!', ',', '~', '+', '=', '(', ')' };

        private static readonly Regex[] DifficultyRegexes = {
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_1\.csv$"),
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_2\.csv$"),
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_3\.csv$"),
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_4\.csv$"),
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_5\.csv$")
        };

    }
}
