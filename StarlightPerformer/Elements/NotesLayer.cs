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
using Brush = SharpDX.Direct2D1.Brush;
using Pen = StarlightPerformer.Stage.Pen;

namespace StarlightPerformer.Elements {
    public sealed class NotesLayer : Element {

        public NotesLayer(Score score) {
            _score = score;
        }

        public override void OnGotContext(RenderContext context) {
            base.OnGotContext(context);

            CeilingPen = new Pen(Color.Red, context);
            HoldLinePen = new Pen(Color.Yellow, context);
            SyncLinePen = new Pen(Color.DodgerBlue, context);
            FlickLinePen = new Pen(Color.OliveDrab, context);
            SlideLinePen = new Pen(Color.LightPink, context);

            var t = context.RenderTarget;
            NoteCommonStroke = new Pen(Color.FromArgb(0x22, 0x22, 0x22), NoteShapeStrokeWidth, context);
            NoteCommonFill = new SolidColorBrush(t, Color.White.ColorToRC4());
            TapNoteShapeStroke = new Pen(Color.FromArgb(0xFF, 0x33, 0x66), NoteShapeStrokeWidth, context);
            HoldNoteShapeStroke = new Pen(Color.FromArgb(0xFF, 0xBB, 0x22), NoteShapeStrokeWidth, context);
            HoldNoteShapeFillInner = new SolidColorBrush(t, Color.White.ColorToRC4());
            FlickNoteShapeStroke = new Pen(Color.FromArgb(0x22, 0x55, 0xBB), NoteShapeStrokeWidth, context);
            FlickNoteShapeFillInner = new SolidColorBrush(t, Color.White.ColorToRC4());
            SlideNoteShapeFillInner = new SolidColorBrush(t, Color.White.ColorToRC4());
        }

        protected override void DrawInternal(GameTime gameTime, RenderContext context) {
            base.DrawInternal(gameTime, context);
            DrawNotes(context, gameTime.Total.TotalSeconds, _score.Notes, 0, _score.Notes.Count);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                CeilingPen.Dispose();
                HoldLinePen.Dispose();
                SyncLinePen.Dispose();
                FlickLinePen.Dispose();
                SyncLinePen.Dispose();

                NoteCommonStroke.Dispose();
                NoteCommonFill.Dispose();
                TapNoteShapeStroke.Dispose();
                HoldNoteShapeStroke.Dispose();
                HoldNoteShapeFillInner.Dispose();
                FlickNoteShapeStroke.Dispose();
                FlickNoteShapeFillInner.Dispose();
                SlideNoteShapeFillInner.Dispose();
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

        public static void DrawSelectedRect(RenderContext context, double now, Note note, Pen pen) {
            float x = GetNoteXPosition(context, now, note), y = GetNoteYPosition(context, now, note);
            float r = GetNoteRadius(context, now, note);
            context.DrawRectangle(pen, x - r, y - r, r * 2f, r * 2f);
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
            DrawHoldLine(context, now, startNote, endNote, HoldLinePen);
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
            float xcontrol1, xcontrol2, ycontrol1, ycontrol2;
            GetBezierFromQuadratic(x1, xmid, x2, out xcontrol1, out xcontrol2);
            GetBezierFromQuadratic(y1, ymid, y2, out ycontrol1, out ycontrol2);
            context.DrawBezier(pen, x1, y1, xcontrol1, ycontrol1, xcontrol2, ycontrol2, x2, y2);
        }

        public static void DrawSlideLine(RenderContext context, double now, Note startNote, Note endNote) {
            if (endNote.IsFlick) {
                DrawFlickLine(context, now, startNote, endNote);
                return;
            }
            if (startNote.IsSlideEnd || IsNoteOnStage(startNote, now)) {
                DrawHoldLine(context, now, startNote, endNote, SlideLinePen);
                return;
            }
            if (IsNotePassed(startNote, now)) {
                var nextSlideNote = startNote.NextFlickOrSlide;
                if (nextSlideNote == null) {
                    // Actually, here is an example of invalid format. :)
                    DrawHoldLine(context, now, startNote, endNote, SlideLinePen);
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
                context.DrawBezier(SlideLinePen, x1, y1, xcontrol1, ycontrol1, xcontrol2, ycontrol2, x2, y2);
            }
        }

        public static void DrawFlickLine(RenderContext context, double now, Note startNote, Note endNote) {
            DrawSimpleLine(context, now, startNote, endNote, FlickLinePen);
        }

        public static void DrawSimpleLine(RenderContext context, double now, Note startNote, Note endNote, Pen pen) {
            OnStageStatus s1 = GetNoteOnStageStatus(startNote, now), s2 = GetNoteOnStageStatus(endNote, now);
            if (s1 != OnStageStatus.OnStage && s2 != OnStageStatus.OnStage && s1 == s2) {
                return;
            }
            float x1, x2, y1, y2;
            GetNotePairPositions(context, now, startNote, endNote, out x1, out x2, out y1, out y2);
            context.DrawLine(pen, x1, y1, x2, y2);
        }

        public static void DrawCommonNoteOutline(RenderContext context, float x, float y, float r) {
            context.FillEllipse(NoteCommonFill, x - r, y - r, r * 2, r * 2);
            context.DrawEllipse(NoteCommonStroke, x - r, y - r, r * 2, r * 2);
        }

        public static void DrawTapNote(RenderContext context, double now, Note note) {
            if (!IsNoteOnStage(note, now)) {
                return;
            }
            float x = GetNoteXPosition(context, now, note),
                y = GetNoteYPosition(context, now, note),
                r = GetNoteRadius(context, now, note);
            DrawCommonNoteOutline(context, x, y, r);

            var r1 = r * ScaleFactor1;
            using (var fill = GetFillBrush(context.RenderTarget, x, y, r, TapNoteShapeFillColors)) {
                context.FillEllipse(fill.Brush, x - r1, y - r1, r1 * 2, r1 * 2);
            }
            context.DrawEllipse(TapNoteShapeStroke, x - r1, y - r1, r1 * 2, r1 * 2);
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
            DrawCommonNoteOutline(context, x, y, r);

            var r1 = r * ScaleFactor1;
            // Triangle
            var polygon = new PointF[3];
            if (note.FlickType == NoteFlickType.Left) {
                polygon[0] = new PointF(x - r1, y);
                polygon[1] = new PointF(x + r1 / 2, y + r1 / 2 * Sqrt3);
                polygon[2] = new PointF(x + r1 / 2, y - r1 / 2 * Sqrt3);

            } else if (note.FlickType == NoteFlickType.Right) {
                polygon[0] = new PointF(x + r1, y);
                polygon[1] = new PointF(x - r1 / 2, y - r1 / 2 * Sqrt3);
                polygon[2] = new PointF(x - r1 / 2, y + r1 / 2 * Sqrt3);
            }
            using (var fill = GetFillBrush(context.RenderTarget, x, y, r, FlickNoteShapeFillOuterColors)) {
                context.FillPolygon(fill.Brush, polygon);
            }
            context.DrawPolygon(FlickNoteShapeStroke, polygon);
        }

        public static void DrawHoldNote(RenderContext context, double now, Note note) {
            if (!IsNoteOnStage(note, now)) {
                return;
            }
            float x = GetNoteXPosition(context, now, note),
                y = GetNoteYPosition(context, now, note),
                r = GetNoteRadius(context, now, note);
            DrawCommonNoteOutline(context, x, y, r);

            var r1 = r * ScaleFactor1;
            using (var fill = GetFillBrush(context.RenderTarget, x, y, r, HoldNoteShapeFillOuterColors)) {
                context.FillEllipse(fill.Brush, x - r1, y - r1, r1 * 2, r1 * 2);
            }
            context.DrawEllipse(HoldNoteShapeStroke, x - r1, y - r1, r1 * 2, r1 * 2);
            var r2 = r * ScaleFactor3;
            context.FillEllipse(HoldNoteShapeFillInner, x - r2, y - r2, r2 * 2, r2 * 2);
        }

        public static void DrawSlideNote(RenderContext context, double now, Note note) {
            if (note.FlickType != NoteFlickType.None) {
                DrawFlickNote(context, now, note);
                return;
            }

            float x, y, r;
            Color[] fillColors;
            if (note.IsSlideEnd || IsNoteOnStage(note, now)) {
                x = GetNoteXPosition(context, now, note);
                y = GetNoteYPosition(context, now, note);
                r = GetNoteRadius(context, now, note);
                fillColors = note.IsSlideMidway ? SlideNoteShapeFillOuterTranslucentColors : SlideNoteShapeFillOuterColors;
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
                    fillColors = SlideNoteShapeFillOuterColors;
                }
            } else {
                return;
            }

            DrawCommonNoteOutline(context, x, y, r);
            var r1 = r * ScaleFactor1;
            using (var fill = GetFillBrush(context.RenderTarget, x, y, r, fillColors)) {
                context.FillEllipse(fill.Brush, x - r1, y - r1, r1 * 2, r1 * 2);
            }
            var r2 = r * ScaleFactor3;
            context.FillEllipse(SlideNoteShapeFillInner, x - r2, y - r2, r2 * 2, r2 * 2);
            var l = r * SlideNoteStrikeHeightFactor;
            context.FillRectangle(SlideNoteShapeFillInner, x - r1 - 1, y - l, r1 * 2 + 2, l * 2);
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

        public static float GetNoteRadius(RenderContext context, double now, Note note) {
            var timeRemaining = note.HitTiming - now;
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

        public static float FutureTimeWindow = 1f;
        public static readonly float PastTimeWindow = 0.2f;
        public static readonly float AvatarCircleDiameter = 50;
        public static readonly float AvatarCircleRadius = AvatarCircleDiameter / 2;
        public static readonly float[] AvatarCenterXStartPositions = { 0.272363281f, 0.381347656f, 0.5f, 0.618652344f, 0.727636719f };
        public static readonly float[] AvatarCenterXEndPositions = { 0.192382812f, 0.346191406f, 0.5f, 0.653808594f, 0.807617188f };
        public static readonly float BaseLineYPosition = 0.828125f;
        // Then we know the bottom is <BaseLineYPosition + (PastWindow / FutureWindow) * (BaseLineYPosition - Ceiling))>.
        public static readonly float FutureNoteCeiling = 0.21875f;

        private static readonly float NoteShapeStrokeWidth = 1;

        private static readonly float ScaleFactor1 = 0.8f;
        private static readonly float ScaleFactor2 = 0.5f;
        private static readonly float ScaleFactor3 = (float)1 / 3f;
        private static readonly float SlideNoteStrikeHeightFactor = (float)4 / 30;

        private static Pen CeilingPen { get; set; }
        private static Pen HoldLinePen { get; set; }
        private static Pen SyncLinePen { get; set; }
        private static Pen FlickLinePen { get; set; }
        private static Pen SlideLinePen { get; set; }

        private static Pen NoteCommonStroke { get; set; }
        private static Brush NoteCommonFill { get; set; }
        private static Pen TapNoteShapeStroke { get; set; }
        private static Color[] TapNoteShapeFillColors { get; } = { Color.FromArgb(0xFF, 0x99, 0xBB), Color.FromArgb(0xFF, 0x33, 0x66) };
        private static Pen HoldNoteShapeStroke { get; set; }
        private static Color[] HoldNoteShapeFillOuterColors { get; } = { Color.FromArgb(0xFF, 0xDD, 0x66), Color.FromArgb(0xFF, 0xBB, 0x22) };
        private static Brush HoldNoteShapeFillInner { get; set; }
        private static Pen FlickNoteShapeStroke { get; set; }
        private static Color[] FlickNoteShapeFillOuterColors { get; } = { Color.FromArgb(0x88, 0xBB, 0xFF), Color.FromArgb(0x22, 0x55, 0xBB) };
        private static Brush FlickNoteShapeFillInner { get; set; }
        private static Color[] SlideNoteShapeFillOuterColors { get; } = { Color.FromArgb(0xA5, 0x46, 0xDA), Color.FromArgb(0xE1, 0xA8, 0xFB) };
        private static Color[] SlideNoteShapeFillOuterTranslucentColors { get; } = { Color.FromArgb(0x80, 0xA5, 0x46, 0xDA), Color.FromArgb(0x80, 0xE1, 0xA8, 0xFB) };
        private static Brush SlideNoteShapeFillInner { get; set; }

        private static LinearGradientBrushEx GetFillBrush(RenderTarget target, float x, float y, float r, Color[] colors) {
            var r1 = r * ScaleFactor1;
            var top = y - r1;
            var bottom = y + r1;
            var props = new LinearGradientBrushProperties {
                StartPoint = new RawVector2(0, top),
                EndPoint = new RawVector2(0, bottom)
            };
            var gradientStops = new List<GradientStop>();
            var count = colors.Length;
            for (var i = 0; i < count; ++i) {
                var gs = new GradientStop {
                    Color = colors[i].ColorToRC4(),
                    Position = (float)i / count
                };
                gradientStops.Add(gs);
            }
            var collection = new GradientStopCollection(target, gradientStops.ToArray(), ExtendMode.Wrap);
            return new LinearGradientBrushEx(target, props, collection);
        }

        private static readonly float Sqrt3 = (float)Math.Sqrt(3);

        private readonly Score _score;

    }
}
