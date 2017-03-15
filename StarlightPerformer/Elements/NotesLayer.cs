using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using StarlightPerformer.Beatmap;
using StarlightPerformer.Core;
using StarlightPerformer.Stage;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Pen = StarlightPerformer.Stage.Pen;

namespace StarlightPerformer.Elements {
    public sealed class NotesLayer : Element {

        public NotesLayer(Score score) {
            _score = score;
        }

        public override void OnGotContext(RenderContext context) {
            base.OnGotContext(context);

            NotesImage = D2DHelper.LoadBitmap(NotesBitmapFilePath, context.RenderTarget);

            var size = context.ClientSize;
            var props = new LinearGradientBrushProperties {
                StartPoint = new RawVector2(size.Height, 0),
                EndPoint = new RawVector2(0, size.Height)
            };
            var gradientStops = new List<GradientStop>();
            var colorCount = GradientColors.Length;
            for (var i = 0; i < colorCount; ++i) {
                gradientStops.Add(new GradientStop {
                    Color = GradientColors[i].ColorToRC4(),
                    Position = (float)i / (colorCount - 1)
                });
            }
            var collection = new GradientStopCollection(context.RenderTarget, gradientStops.ToArray(), ExtendMode.Wrap);
            ConnectionBrush = new LinearGradientBrushEx(context.RenderTarget, props, collection);
            ConnectionPen = new Pen(ConnectionBrush.Brush, ConnectionStrokeWidth);

            SyncLinePen = new Pen(Color.White, 3, context);
        }

        protected override void DrawInternal(GameTime gameTime, RenderContext context) {
            base.DrawInternal(gameTime, context);
            DrawNotes(context, gameTime.Total.TotalSeconds, _score.Notes, 0, _score.Notes.Count);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                NotesImage.Dispose();

                ConnectionPen.Dispose();
                ConnectionBrush.Dispose();
                SyncLinePen.Dispose();
            }
        }

        public static void DrawNotes(RenderContext context, double now, IReadOnlyList<Note> notes, int startIndex, int endIndex) {
            if (startIndex < 0) {
                return;
            }
            var selectedNotes = notes.Skip(startIndex).Take(endIndex - startIndex + 1);
            foreach (var note in selectedNotes) {
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                    case NoteType.Hold:
                    case NoteType.Slide:
                        if (note.IsSync) {
                            DrawSyncLine(context, now, note, note.SyncPair);
                        }
                        break;
                }
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                        if (note.IsFlick) {
                            if (note.HasNextFlickOrSlide) {
                                DrawFlickLine(context, now, note, note.NextFlickOrSlide);
                            }
                        }
                        break;
                    case NoteType.Hold:
                        if (note.IsHoldStart) {
                            DrawHoldLine(context, now, note, note.HoldPair);
                        }
                        if (note.IsHoldEnd) {
                            if (!IsNoteOnStage(note.HoldPair, now)) {
                                DrawHoldLine(context, now, note.HoldPair, note);
                            }
                        }
                        break;
                    case NoteType.Slide:
                        if (note.HasNextFlickOrSlide) {
                            DrawSlideLine(context, now, note, note.NextFlickOrSlide);
                        }
                        if (note.HasPrevFlickOrSlide) {
                            if (!IsNoteOnStage(note.PrevFlickOrSlide, now)) {
                                DrawSlideLine(context, now, note.PrevFlickOrSlide, note);
                            }
                        }
                        break;
                }
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                        if (note.FlickType == NoteFlickType.None) {
                            if (note.IsHoldEnd) {
                                DrawHoldNote(context, now, note);
                            } else {
                                DrawTapNote(context, now, note);
                            }
                        } else {
                            DrawFlickNote(context, now, note);
                        }
                        break;
                    case NoteType.Hold:
                        DrawHoldNote(context, now, note);
                        break;
                    case NoteType.Slide:
                        DrawSlideNote(context, now, note);
                        break;
                }
            }
        }

        public static void DrawSyncLine(RenderContext context, double now, Note note1, Note note2) {
            if (!IsNoteOnStage(note1, now) || !IsNoteOnStage(note2, now)) {
                return;
            }
            float x1 = GetNoteXPosition(context, now, note1),
                y = GetNoteYPosition(context, now, note2),
                x2 = GetNoteXPosition(context, now, note2);
            float r = GetNoteRadius(context, now, note2);
            float xLeft = Math.Min(x1, x2), xRight = Math.Max(x1, x2);
            context.DrawLine(SyncLinePen, xLeft + r, y, xRight - r, y);
        }

        public static void DrawHoldLine(RenderContext context, double now, Note startNote, Note endNote) {
            DrawHoldLine(context, now, startNote, endNote, ConnectionPen);
        }

        public static void DrawHoldLine(RenderContext context, double now, Note startNote, Note endNote, Pen pen) {
            OnStageStatus s1 = GetNoteOnStageStatus(startNote, now), s2 = GetNoteOnStageStatus(endNote, now);
            if (s1 == s2 && s1 != OnStageStatus.OnStage) {
                return;
            }
            float t1 = GetNoteTransformedTime(context, now, startNote, true, true);
            float t2 = GetNoteTransformedTime(context, now, endNote, true, true);
            float tmid = (t1 + t2) * 0.5f;
            float x1 = GetNoteXPosition(context, startNote.FinishPosition, startNote.StartPosition, t1);
            float x2 = GetNoteXPosition(context, endNote.FinishPosition, endNote.StartPosition, t2);
            float xmid = GetNoteXPosition(context, endNote.FinishPosition, endNote.StartPosition, tmid);
            float y1 = GetNoteYPosition(context, t1);
            float y2 = GetNoteYPosition(context, t2);
            float ymid = GetNoteYPosition(context, tmid);
            float r1 = GetNoteRadius(context, now, startNote);
            float r2 = GetNoteRadius(context, now, endNote);
            float rmid = GetNoteRadius(context, now, (startNote.HitTiming + endNote.HitTiming) / 2);

            var xl1 = x1 - r1;
            var xl2 = x2 - r2;
            var xr1 = x1 + r1;
            var xr2 = x2 + r2;
            var xlm = xmid - rmid;
            var xrm = xmid + rmid;

            GetBezierFromQuadratic(xl1, xlm, xl2, out var xlc1, out var xlc2);
            GetBezierFromQuadratic(xr1, xrm, xr2, out var xrc1, out var xrc2);
            GetBezierFromQuadratic(y1, ymid, y2, out var yc1, out var yc2);

            using (var path = new PathGeometry(context.RenderTarget.Factory)) {
                using (var s = path.Open()) {
                    s.SetFillMode(FillMode.Winding);
                    s.BeginFigure(new RawVector2(xl1, y1), FigureBegin.Filled);
                    s.AddBezier(new BezierSegment {
                        Point1 = new RawVector2(xlc1, yc1),
                        Point2 = new RawVector2(xlc2, yc2),
                        Point3 = new RawVector2(xl2, y2)
                    });
                    s.AddLine(new RawVector2(xr2, y2));
                    s.AddBezier(new BezierSegment {
                        Point1 = new RawVector2(xrc2, yc2),
                        Point2 = new RawVector2(xrc1, yc1),
                        Point3 = new RawVector2(xr1, y1)
                    });
                    s.AddLine(new RawVector2(xl1, y1));
                    s.EndFigure(FigureEnd.Closed);
                    s.Close();
                }
                context.RenderTarget.FillGeometry(path, ConnectionBrush.Brush);
            }
        }

        public static void DrawSlideLine(RenderContext context, double now, Note startNote, Note endNote) {
            if (endNote.IsFlick) {
                DrawFlickLine(context, now, startNote, endNote);
                return;
            }
            if (startNote.IsSlideEnd || IsNoteOnStage(startNote, now)) {
                DrawHoldLine(context, now, startNote, endNote, ConnectionPen);
                return;
            }
            if (IsNotePassed(startNote, now)) {
                var nextSlideNote = startNote.NextFlickOrSlide;
                if (nextSlideNote == null) {
                    // Actually, here is an example of invalid format. :)
                    DrawHoldLine(context, now, startNote, endNote, ConnectionPen);
                    return;
                }
                if (IsNotePassed(nextSlideNote, now)) {
                    return;
                }
                var startX = GetEndXByNotePosition(context.ClientSize, startNote.FinishPosition);
                var endX = GetEndXByNotePosition(context.ClientSize, nextSlideNote.FinishPosition);
                var y1 = GetAvatarYPosition(context.ClientSize);
                var x1 = (float)((now - startNote.HitTiming) / (nextSlideNote.HitTiming - startNote.HitTiming)) * (endX - startX) + startX;
                float t1 = GetNoteTransformedTime(context, now, startNote, true, true);
                float t2 = GetNoteTransformedTime(context, now, endNote, true, true);
                float tmid = (t1 + t2) * 0.5f;
                float x2 = GetNoteXPosition(context, endNote.FinishPosition, endNote.StartPosition, t2);
                float xmid = GetNoteXPosition(context, endNote.FinishPosition, endNote.StartPosition, tmid);
                float y2 = GetNoteYPosition(context, t2);
                float ymid = GetNoteYPosition(context, tmid);
                float xcontrol1, xcontrol2, ycontrol1, ycontrol2;
                GetBezierFromQuadratic(x1, xmid, x2, out xcontrol1, out xcontrol2);
                GetBezierFromQuadratic(y1, ymid, y2, out ycontrol1, out ycontrol2);
                context.DrawBezier(ConnectionPen, x1, y1, xcontrol1, ycontrol1, xcontrol2, ycontrol2, x2, y2);
            }
        }

        public static void DrawFlickLine(RenderContext context, double now, Note startNote, Note endNote) {
            OnStageStatus s1 = GetNoteOnStageStatus(startNote, now), s2 = GetNoteOnStageStatus(endNote, now);
            if (s1 != OnStageStatus.OnStage && s2 != OnStageStatus.OnStage && s1 == s2) {
                return;
            }
            float x1, x2, y1, y2;
            GetNotePairPositions(context, now, startNote, endNote, out x1, out x2, out y1, out y2);
            context.DrawLine(ConnectionPen, x1, y1, x2, y2);
        }

        public static void DrawTapNote(RenderContext context, double now, Note note) {
            if (!IsNoteOnStage(note, now)) {
                return;
            }
            float x = GetNoteXPosition(context, now, note),
                y = GetNoteYPosition(context, now, note),
                r = GetNoteRadius(context, now, note);
            DrawNote(context, note, x, y, r);
        }

        public static void DrawFlickNote(RenderContext context, double now, Note note) {
            if (!IsNoteOnStage(note, now)) {
                return;
            }
            if (note.FlickType == NoteFlickType.None) {
                Debug.Print("WARNING: Tap/hold/slide note requested in DrawFlickNote.");
                return;
            }
            float x = GetNoteXPosition(context, now, note),
                y = GetNoteYPosition(context, now, note),
                r = GetNoteRadius(context, now, note);

            DrawNote(context, note, x, y, r);
        }

        public static void DrawHoldNote(RenderContext context, double now, Note note) {
            if (!IsNoteOnStage(note, now)) {
                return;
            }
            float x = GetNoteXPosition(context, now, note),
                y = GetNoteYPosition(context, now, note),
                r = GetNoteRadius(context, now, note);
            DrawNote(context, note, x, y, r);
        }

        public static void DrawSlideNote(RenderContext context, double now, Note note) {
            if (note.FlickType != NoteFlickType.None) {
                DrawFlickNote(context, now, note);
                return;
            }

            float x, y, r;
            if (note.IsSlideEnd || IsNoteOnStage(note, now)) {
                x = GetNoteXPosition(context, now, note);
                y = GetNoteYPosition(context, now, note);
                r = GetNoteRadius(context, now, note);
            } else if (IsNotePassed(note, now)) {
                if (!note.HasNextFlickOrSlide || IsNotePassed(note.NextFlickOrSlide, now)) {
                    return;
                }
                var nextSlideNote = note.NextFlickOrSlide;
                if (nextSlideNote == null) {
                    // Actually, here is an example of invalid format. :)
                    DrawTapNote(context, now, note);
                    return;
                } else {
                    var startX = GetEndXByNotePosition(context.ClientSize, note.FinishPosition);
                    var endX = GetEndXByNotePosition(context.ClientSize, nextSlideNote.FinishPosition);
                    y = GetAvatarYPosition(context.ClientSize);
                    x = (float)((now - note.HitTiming) / (nextSlideNote.HitTiming - note.HitTiming)) * (endX - startX) + startX;
                    r = AvatarCircleRadius;
                }
            } else {
                return;
            }

            DrawNote(context, note, x, y, r);
        }

        public static void GetBezierFromQuadratic(float x1, float xmid, float x4, out float x2, out float x3) {
            float xcontrol = xmid * 2f - (x1 + x4) * 0.5f;
            x2 = (x1 + xcontrol * 2f) / 3f;
            x3 = (x4 + xcontrol * 2f) / 3f;
        }

        public static void GetNotePairPositions(RenderContext context, double now, Note note1, Note note2, out float x1, out float x2, out float y1, out float y2) {
            var clientSize = context.ClientSize;
            if (IsNotePassed(note1, now)) {
                x1 = GetEndXByNotePosition(clientSize, note1.FinishPosition);
                y1 = GetAvatarYPosition(clientSize);
            } else if (IsNoteComing(note1, now)) {
                x1 = GetStartXByNotePosition(clientSize, note1.FinishPosition);
                y1 = GetBirthYPosition(clientSize);
            } else {
                x1 = GetNoteXPosition(context, now, note1);
                y1 = GetNoteYPosition(context, now, note1);
            }
            if (IsNotePassed(note2, now)) {
                x2 = GetEndXByNotePosition(clientSize, note2.FinishPosition);
                y2 = GetAvatarYPosition(clientSize);
            } else if (IsNoteComing(note2, now)) {
                x2 = GetStartXByNotePosition(clientSize, note2.FinishPosition);
                y2 = GetBirthYPosition(clientSize);
            } else {
                x2 = GetNoteXPosition(context, now, note2);
                y2 = GetNoteYPosition(context, now, note2);
            }
        }

        public static float NoteTimeTransform(float timeRemainingInWindow) {
            return timeRemainingInWindow / (2f - timeRemainingInWindow);
        }

        public static float NoteXTransform(float timeTransformed) {
            return timeTransformed;
        }

        public static float NoteYTransform(float timeTransformed) {
            return timeTransformed + 2.05128205f * timeTransformed * (1f - timeTransformed);
        }

        public static float GetNoteTransformedTime(RenderContext context, double now, Note note, bool clampComing = false, bool clampPassed = false) {
            var timeRemaining = note.HitTiming - now;
            var timeRemainingInWindow = (float)timeRemaining / FutureTimeWindow;
            if (clampComing && timeRemaining > FutureTimeWindow) {
                timeRemainingInWindow = 1f;
            }
            if (clampPassed && timeRemaining < 0f) {
                timeRemainingInWindow = 0f;
            }
            return NoteTimeTransform(timeRemainingInWindow);
        }

        public static float GetNoteXPosition(RenderContext context, double now, Note note, bool clampComing = false, bool clampPassed = false) {
            var timeTransformed = GetNoteTransformedTime(context, now, note, clampComing, clampPassed);
            return GetNoteXPosition(context, note.FinishPosition, note.StartPosition, timeTransformed);
        }

        public static float GetNoteXPosition(RenderContext context, NotePosition finishPosition, NotePosition startPosition, float timeTransformed) {
            var clientSize = context.ClientSize;
            var endPos = AvatarCenterXEndPositions[(int)finishPosition - 1] * clientSize.Width;
            var displayStartPosition = finishPosition;
            var startPos = AvatarCenterXStartPositions[(int)displayStartPosition - 1] * clientSize.Width;
            return endPos - (endPos - startPos) * NoteXTransform(timeTransformed);
        }

        public static float GetNoteYPosition(RenderContext context, double now, Note note, bool clampComing = false, bool clampPassed = false) {
            var timeTransformed = GetNoteTransformedTime(context, now, note, clampComing, clampPassed);
            return GetNoteYPosition(context, timeTransformed);
        }

        public static float GetNoteYPosition(RenderContext context, float timeTransformed) {
            var clientSize = context.ClientSize;
            float ceiling = FutureNoteCeiling * clientSize.Height,
                baseLine = BaseLineYPosition * clientSize.Height;
            return baseLine - (baseLine - ceiling) * NoteYTransform(timeTransformed);
        }

        public static float GetNoteRadius(RenderContext context, double now, Note note) => GetNoteRadius(context, now, note.HitTiming);

        public static float GetNoteRadius(RenderContext context, double now, double hitTiming) {
            var timeRemaining = hitTiming - now;
            var timeTransformed = NoteTimeTransform((float)timeRemaining / FutureTimeWindow);
            if (timeTransformed < 0.75f) {
                if (timeTransformed < 0f) {
                    return AvatarCircleRadius;
                } else {
                    return AvatarCircleRadius * (1f - timeTransformed * 0.933333333f);
                }
            } else {
                if (timeTransformed < 1f) {
                    return AvatarCircleRadius * ((1f - timeTransformed) * 1.2f);
                } else {
                    return 0f;
                }
            }
        }

        public static float GetAvatarXPosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXEndPositions[(int)position - 1];
        }

        public static float GetAvatarYPosition(Size clientSize) {
            return clientSize.Height * BaseLineYPosition;
        }

        public static float GetStartXByNotePosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXStartPositions[(int)position - 1];
        }

        public static float GetEndXByNotePosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXEndPositions[(int)position - 1];
        }

        public static float GetBirthYPosition(Size clientSize) {
            return clientSize.Height * FutureNoteCeiling;
        }

        public static OnStageStatus GetNoteOnStageStatus(Note note, double now) {
            if (note.HitTiming < now) {
                return OnStageStatus.Passed;
            }
            if (note.HitTiming > now + FutureTimeWindow) {
                return OnStageStatus.Upcoming;
            }
            return OnStageStatus.OnStage;
        }

        public static bool IsNoteOnStage(Note note, double now) {
            return now <= note.HitTiming && note.HitTiming <= now + FutureTimeWindow;
        }

        public static bool IsNotePassed(Note note, double now) {
            return note.HitTiming < now;
        }

        public static bool IsNoteComing(Note note, double now) {
            return note.HitTiming > now + FutureTimeWindow;
        }

        public enum OnStageStatus {
            Upcoming,
            OnStage,
            Passed
        }

        private static (float x, float y) GetIconLocation(SongColor songColor, Note note) {
            var column = 3;
            switch (songColor) {
                case SongColor.All:
                    column = 3;
                    break;
                case SongColor.Cute:
                    column = 0;
                    break;
                case SongColor.Cool:
                    column = 1;
                    break;
                case SongColor.Passion:
                    column = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(songColor));
            }
            var row = 0;
            if (note.IsTap) {
                row = 0;
            } else if (note.IsHoldStart) {
                row = 1;
            } else if (note.FlickType == NoteFlickType.Left) {
                row = 2;
            } else if (note.FlickType == NoteFlickType.Right) {
                row = 3;
            } /*else if (note.ShouldBeRenderedAsSlide) {
                // Not implemented, don't use.
                row = 4;
            }*/

            return (x: ImageWidth * column, y: ImageHeight * row);
        }

        private static void DrawNote(RenderContext context, Note note, float x, float y, float r) {
            var w = r * 2 * ImageAspectRatio;
            var h = r * 2;
            x -= w / 2;
            y -= h / 2;

            var loc = GetIconLocation(DefaultSongColor, note);
            context.DrawImage(NotesImage, x, y, w, h, loc.x, loc.y, ImageWidth, ImageHeight);
        }

        public static float FutureTimeWindow = 1f;
        public static readonly float PastTimeWindow = 0.2f;
        public static readonly float AvatarCircleDiameter = 110;
        public static readonly float AvatarCircleRadius = AvatarCircleDiameter / 2;
        public static readonly float[] AvatarCenterXStartPositions = { 0.3f, 0.4f, 0.5f, 0.6f, 0.7f };
        public static readonly float[] AvatarCenterXEndPositions = { 0.18f, 0.34f, 0.5f, 0.66f, 0.82f };
        public static readonly float BaseLineYPosition = 0.828125f;
        // Then we know the bottom is <BaseLineYPosition + (PastWindow / FutureWindow) * (BaseLineYPosition - Ceiling))>.
        public static readonly float FutureNoteCeiling = 0.21875f;

        private static readonly string NotesBitmapFilePath = "Resources/images/notes/notes.png";
        private static readonly float ImageWidth = 154;
        private static readonly float ImageHeight = 110;
        private static readonly float ImageAspectRatio = ImageWidth / ImageHeight;

        private static Bitmap NotesImage { get; set; }

        private static readonly SongColor DefaultSongColor = SongColor.All;

        private static LinearGradientBrushEx ConnectionBrush { get; set; }
        private static Pen ConnectionPen { get; set; }

        private static readonly float ConnectionStrokeWidth = 55;
        private const int ConnectionAlpha = 63;

        private static readonly Color[] GradientColors = {
            Color.FromArgb(ConnectionAlpha, 255, 0, 0),
            Color.FromArgb(ConnectionAlpha, 255, 165, 0),
            Color.FromArgb(ConnectionAlpha, 255, 255, 0),
            Color.FromArgb(ConnectionAlpha, 0, 255, 0),
            Color.FromArgb(ConnectionAlpha, 0, 255, 255),
            Color.FromArgb(ConnectionAlpha, 0, 0, 255),
            Color.FromArgb(ConnectionAlpha, 43, 0, 255),
            Color.FromArgb(ConnectionAlpha, 87, 0, 255),
            Color.FromArgb(ConnectionAlpha, 255, 0, 0),
        };

        private static Pen SyncLinePen { get; set; }

        private readonly Score _score;

    }
}
