using System.Drawing;
using StarlightPerformer.Beatmap;
using StarlightPerformer.Elements.Internal;
using StarlightPerformer.Stage;

namespace StarlightPerformer.Elements {
    partial class NotesLayer {

        private void DrawHoldRibbon(RenderContext context, double now, Note startNote, Note endNote) {
            OnStageStatus s1 = NotesLayerUtils.GetNoteOnStageStatus(startNote, now), s2 = NotesLayerUtils.GetNoteOnStageStatus(endNote, now);
            if (s1 == s2 && s1 != OnStageStatus.OnStage) {
                return;
            }

            var mesh = new RibbonMesh(context, startNote, endNote, now, ConnectionType.Hold);
            mesh.Fill(RibbonBrush.Brush);
        }

        private void DrawSlideRibbon(RenderContext context, double now, Note startNote, Note endNote) {
            if (endNote.IsFlick) {
                DrawFlickRibbon(context, now, startNote, endNote);
                return;
            }
            if (startNote.IsSlideEnd || NotesLayerUtils.IsNoteOnStage(startNote, now)) {
                DrawHoldRibbon(context, now, startNote, endNote);
                return;
            }
            if (NotesLayerUtils.IsNotePassed(startNote, now)) {
                var nextSlideNote = startNote.NextFlickOrSlide;
                if (nextSlideNote == null) {
                    // Actually, here is an example of invalid format. :)
                    DrawHoldRibbon(context, now, startNote, endNote);
                    return;
                }
                if (NotesLayerUtils.IsNotePassed(nextSlideNote, now)) {
                    return;
                }
                var startX = NotesLayerUtils.GetEndXByNotePosition(context.ClientSize, startNote.FinishPosition);
                var endX = NotesLayerUtils.GetEndXByNotePosition(context.ClientSize, nextSlideNote.FinishPosition);
                var y1 = NotesLayerUtils.GetAvatarYPosition(context.ClientSize);
                var x1 = (float)((now - startNote.HitTiming) / (nextSlideNote.HitTiming - startNote.HitTiming)) * (endX - startX) + startX;
                float t1 = NotesLayerUtils.GetNoteTransformedTime(now, startNote, clampComing: true, clampPassed: true);
                float t2 = NotesLayerUtils.GetNoteTransformedTime(now, endNote, clampComing: true, clampPassed: true);
                float tmid = (t1 + t2) * 0.5f;
                float x2 = NotesLayerUtils.GetNoteXPosition(context, endNote.FinishPosition, t2);
                float xmid = NotesLayerUtils.GetNoteXPosition(context, endNote.FinishPosition, tmid);
                float y2 = NotesLayerUtils.GetNoteYPosition(context, t2);
                float ymid = NotesLayerUtils.GetNoteYPosition(context, tmid);
                NotesLayerUtils.GetBezierFromQuadratic(x1, xmid, x2, out float xcontrol1, out float xcontrol2);
                NotesLayerUtils.GetBezierFromQuadratic(y1, ymid, y2, out float ycontrol1, out float ycontrol2);
                // TODO:
                //context.DrawBezier(ConnectionPen, x1, y1, xcontrol1, ycontrol1, xcontrol2, ycontrol2, x2, y2);
            }
        }

        private void DrawFlickRibbon(RenderContext context, double now, Note startNote, Note endNote) {
            OnStageStatus s1 = NotesLayerUtils.GetNoteOnStageStatus(startNote, now), s2 = NotesLayerUtils.GetNoteOnStageStatus(endNote, now);
            if (s1 != OnStageStatus.OnStage && s2 != OnStageStatus.OnStage && s1 == s2) {
                return;
            }

            var mesh = new RibbonMesh(context, startNote, endNote, now, ConnectionType.Flick);
            mesh.Fill(RibbonBrush.Brush);
        }

        private static readonly SongColor DefaultSongColor = SongColor.All;

        private LinearGradientBrushEx RibbonBrush { get; set; }

        private const int RibbonAlpha = 63;

        private static readonly Color[] RibbonColors = {
            Color.FromArgb(RibbonAlpha, 255, 0, 0),
            Color.FromArgb(RibbonAlpha, 255, 165, 0),
            Color.FromArgb(RibbonAlpha, 255, 255, 0),
            Color.FromArgb(RibbonAlpha, 0, 255, 0),
            Color.FromArgb(RibbonAlpha, 0, 255, 255),
            Color.FromArgb(RibbonAlpha, 0, 0, 255),
            Color.FromArgb(RibbonAlpha, 43, 0, 255),
            Color.FromArgb(RibbonAlpha, 87, 0, 255),
            Color.FromArgb(RibbonAlpha, 255, 0, 0),
        };

    }
}
