using System.Drawing;
using SharpDX.Direct2D1;
using StarlightPerformer.Core;
using Brush = SharpDX.Direct2D1.Brush;

namespace StarlightPerformer.Stage {
    public sealed class Pen : DisposableBase {

        public Pen(Color color, RenderContext context) {
            Brush = new SolidColorBrush(context.RenderTarget, color.ColorToRC4());
            StrokeWidth = 1;
            StrokeStyle = null;
        }

        public Pen(Color color, float strokeWidth, RenderContext context) {
            Brush = new SolidColorBrush(context.RenderTarget, color.ColorToRC4());
            StrokeWidth = strokeWidth;
            StrokeStyle = null;
        }

        public Pen(Brush brush)
            : this(brush, 1, null) {
        }

        public Pen(Brush brush, float strokeWidth)
            : this(brush, strokeWidth, null) {
        }

        public Pen(Brush brush, float strokeWidth, StrokeStyle strokeStyle) {
            Brush = brush;
            StrokeWidth = strokeWidth;
            StrokeStyle = strokeStyle;
        }

        public Brush Brush { get; }

        public StrokeStyle StrokeStyle { get; }

        public float StrokeWidth { get; }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                Brush.Dispose();
                StrokeStyle?.Dispose();
            }
        }

    }
}
