using System.Diagnostics;
using StarlightPerformer.Beatmap;
using StarlightPerformer.Stage;

namespace StarlightPerformer.Elements {
    partial class NotesLayer {

        private void DrawTapNote(RenderContext context, double now, Note note) {
            if (!NotesLayerUtils.IsNoteOnStage(note, now)) {
                return;
            }
            float x = NotesLayerUtils.GetNoteXPosition(context, now, note),
                y = NotesLayerUtils.GetNoteYPosition(context, now, note),
                r = NotesLayerUtils.GetNoteRadius(now, note);
            DrawNoteImage(context, note, x, y, r);
        }

        private void DrawFlickNote(RenderContext context, double now, Note note) {
            if (!NotesLayerUtils.IsNoteOnStage(note, now)) {
                return;
            }
            if (note.FlickType == NoteFlickType.None) {
                Debug.Print("WARNING: Tap/hold/slide note requested in DrawFlickNote.");
                return;
            }
            float x = NotesLayerUtils.GetNoteXPosition(context, now, note),
                y = NotesLayerUtils.GetNoteYPosition(context, now, note),
                r = NotesLayerUtils.GetNoteRadius(now, note);

            DrawNoteImage(context, note, x, y, r);
        }

        private void DrawHoldNote(RenderContext context, double now, Note note) {
            if (!NotesLayerUtils.IsNoteOnStage(note, now)) {
                return;
            }
            float x = NotesLayerUtils.GetNoteXPosition(context, now, note),
                y = NotesLayerUtils.GetNoteYPosition(context, now, note),
                r = NotesLayerUtils.GetNoteRadius(now, note);
            DrawNoteImage(context, note, x, y, r);
        }

        private void DrawSlideNote(RenderContext context, double now, Note note) {
            if (note.FlickType != NoteFlickType.None) {
                DrawFlickNote(context, now, note);
                return;
            }

            float x, y, r;
            if (note.IsSlideEnd || NotesLayerUtils.IsNoteOnStage(note, now)) {
                x = NotesLayerUtils.GetNoteXPosition(context, now, note);
                y = NotesLayerUtils.GetNoteYPosition(context, now, note);
                r = NotesLayerUtils.GetNoteRadius(now, note);
            } else if (NotesLayerUtils.IsNotePassed(note, now)) {
                if (!note.HasNextFlickOrSlide || NotesLayerUtils.IsNotePassed(note.NextFlickOrSlide, now)) {
                    return;
                }
                var nextSlideNote = note.NextFlickOrSlide;
                if (nextSlideNote == null) {
                    // Actually, here is an example of invalid format. :)
                    DrawTapNote(context, now, note);
                    return;
                } else {
                    var startX = NotesLayerUtils.GetEndXByNotePosition(context.ClientSize, note.FinishPosition);
                    var endX = NotesLayerUtils.GetEndXByNotePosition(context.ClientSize, nextSlideNote.FinishPosition);
                    y = NotesLayerUtils.GetAvatarYPosition(context.ClientSize);
                    x = (float)((now - note.HitTiming) / (nextSlideNote.HitTiming - note.HitTiming)) * (endX - startX) + startX;
                    r = AvatarCircleRadius;
                }
            } else {
                return;
            }

            DrawNoteImage(context, note, x, y, r);
        }

        private void DrawNoteImage(RenderContext context, Note note, float x, float y, float r) {
            var w = r * 2 * ImageAspectRatio;
            var h = r * 2;
            x -= w / 2;
            y -= h / 2;

            var loc = NotesLayerUtils.GetIconLocation(DefaultSongColor, note);
            context.DrawImage(NotesImage, x, y, w, h, loc.x, loc.y, ImageCellWidth, ImageCellHeight);
        }

    }
}
