using System;
using System.Collections.Generic;
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
    public sealed partial class NotesLayer : Element {

        public NotesLayer(Score score) {
            _score = score;
        }

        public override void OnGotContext(RenderContext context) {
            base.OnGotContext(context);

            NotesImage = D2DHelper.LoadBitmap(Definitions.NotesBitmapFilePath, context.RenderTarget);

            var size = context.ClientSize;
            var props = new LinearGradientBrushProperties {
                StartPoint = new RawVector2(size.Height, 0),
                EndPoint = new RawVector2(0, size.Height)
            };
            var gradientStops = new List<GradientStop>();
            var colorCount = RibbonColors.Length;
            for (var i = 0; i < colorCount; ++i) {
                gradientStops.Add(new GradientStop {
                    Color = RibbonColors[i].ColorToRC4(),
                    Position = (float)i / (colorCount - 1)
                });
            }
            var collection = new GradientStopCollection(context.RenderTarget, gradientStops.ToArray(), ExtendMode.Wrap);
            RibbonBrush = new LinearGradientBrushEx(context.RenderTarget, props, collection);

            SyncLinePen = new Pen(Color.White, 3, context);
        }

        protected override void DrawInternal(GameTime gameTime, RenderContext context) {
            base.DrawInternal(gameTime, context);
            DrawNotes(context, gameTime.Total.TotalSeconds, _score.Notes, 0, _score.Notes.Count);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                NotesImage.Dispose();
                RibbonBrush.Dispose();
                SyncLinePen.Dispose();
            }
        }

        private void DrawNotes(RenderContext context, double now, IReadOnlyList<Note> notes, int startIndex, int endIndex) {
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
                                DrawFlickRibbon(context, now, note, note.NextFlickOrSlide);
                            }
                        }
                        break;
                    case NoteType.Hold:
                        if (note.IsHoldStart) {
                            DrawHoldRibbon(context, now, note, note.HoldPair);
                        }
                        if (note.IsHoldEnd) {
                            if (!NotesLayerUtils.IsNoteOnStage(note.HoldPair, now)) {
                                DrawHoldRibbon(context, now, note.HoldPair, note);
                            }
                        }
                        break;
                    case NoteType.Slide:
                        if (note.HasNextFlickOrSlide) {
                            DrawSlideRibbon(context, now, note, note.NextFlickOrSlide);
                        }
                        if (note.HasPrevFlickOrSlide) {
                            if (!NotesLayerUtils.IsNoteOnStage(note.PrevFlickOrSlide, now)) {
                                DrawSlideRibbon(context, now, note.PrevFlickOrSlide, note);
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

        private static void DrawSyncLine(RenderContext context, double now, Note note1, Note note2) {
            if (!NotesLayerUtils.IsNoteOnStage(note1, now) || !NotesLayerUtils.IsNoteOnStage(note2, now)) {
                return;
            }
            float x1 = NotesLayerUtils.GetNoteXPosition(context, now, note1),
                y = NotesLayerUtils.GetNoteYPosition(context, now, note2),
                x2 = NotesLayerUtils.GetNoteXPosition(context, now, note2);
            float r = NotesLayerUtils.GetNoteRadius(now, note2);
            float xLeft = Math.Min(x1, x2), xRight = Math.Max(x1, x2);
            context.DrawLine(SyncLinePen, xLeft + r, y, xRight - r, y);
        }

        private Bitmap NotesImage { get; set; }

        private static Pen SyncLinePen { get; set; }

        private readonly Score _score;

    }
}
