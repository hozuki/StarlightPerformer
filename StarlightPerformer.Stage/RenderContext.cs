using System;
using System.Drawing;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using StarlightPerformer.Core;
using Brush = SharpDX.Direct2D1.Brush;

namespace StarlightPerformer.Stage {
    public sealed class RenderContext : DisposableBase {

        public RenderContext(RenderTarget renderTarget, Size clientSize) {
            RenderTarget = renderTarget;
            ClientSize = clientSize;
        }

        public RenderTarget RenderTarget { get; }

        public Size ClientSize { get; }

        public void BeginDraw() {
            RenderTarget.BeginDraw();
            RenderTarget.Clear(Color.Black.ColorToRC4());
        }

        public void EndDraw() {
            RenderTarget.EndDraw();
        }

        public void DrawBezier(Pen pen, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2) {
            var rt = RenderTarget;
            using (var path = new PathGeometry(rt.Factory)) {
                using (var s = path.Open()) {
                    s.BeginFigure(new RawVector2(x1, y1), FigureBegin.Filled);
                    var segment = new BezierSegment {
                        Point1 = new RawVector2(cx1, cy1),
                        Point2 = new RawVector2(cx2, cy2),
                        Point3 = new RawVector2(x2, y2)
                    };
                    s.AddBezier(segment);
                    s.EndFigure(FigureEnd.Open);
                    s.Close();
                }
                rt.DrawGeometry(path, pen.Brush, pen.StrokeWidth, pen.StrokeStyle);
            }
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2) {
            RenderTarget.DrawLine(new RawVector2(x1, y1), new RawVector2(x2, y2), pen.Brush, pen.StrokeWidth, pen.StrokeStyle);
        }

        public void DrawRectangle(Pen pen, float x, float y, float width, float height) {
            var rect = new RawRectangleF(x, y, x + width, y + height);
            DrawRectangle(pen, rect);
        }

        public void DrawRectangle(Pen pen, RawRectangleF rectangle) {
            RenderTarget.DrawRectangle(rectangle, pen.Brush, pen.StrokeWidth, pen.StrokeStyle);
        }

        public void DrawEllipse(Pen pen, float x, float y, float width, float height) {
            var ellipse = new Ellipse(new RawVector2(x + width / 2, y + height / 2), width / 2, height / 2);
            DrawEllipse(pen, ellipse);
        }

        public void DrawEllipse(Pen pen, Ellipse ellipse) {
            RenderTarget.DrawEllipse(ellipse, pen.Brush, pen.StrokeWidth, pen.StrokeStyle);
        }

        public void DrawPolygon(Pen pen, params PointF[] polygon) {
            using (var path = GetPathFromPolygon(polygon, RenderTarget.Factory)) {
                RenderTarget.DrawGeometry(path, pen.Brush, pen.StrokeWidth, pen.StrokeStyle);
            }
            //var path = GetPathFromPolygon(polygon, RenderTarget.Factory);
            //RenderTarget.DrawGeometry(path, pen.Brush, pen.StrokeWidth, pen.StrokeStyle);
        }

        public void FillEllipse(Brush brush, Ellipse ellipse) {
            RenderTarget.FillEllipse(ellipse, brush);
        }

        public void FillEllipse(Brush brush, float x, float y, float width, float height) {
            var ellipse = new Ellipse(new RawVector2(x + width / 2, y + height / 2), width / 2, height / 2);
            FillEllipse(brush, ellipse);
        }

        public void FillCircle(Brush brush, float x, float y, float r) {
            var ellipse = new Ellipse(new RawVector2(x, y), r, r);
            RenderTarget.FillEllipse(ellipse, brush);
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height) {
            var rect = new RawRectangleF(x, y, x + width, y + height);
            FillRectangle(brush, rect);
        }

        public void FillRectangle(Brush brush, RawRectangleF rectangle) {
            RenderTarget.FillRectangle(rectangle, brush);
        }

        public void FillPolygon(Brush brush, params PointF[] polygon) {
            using (var path = GetPathFromPolygon(polygon, RenderTarget.Factory)) {
                RenderTarget.FillGeometry(path, brush);
            }
            //var path = GetPathFromPolygon(polygon, RenderTarget.Factory);
            //RenderTarget.FillGeometry(path, brush);
        }

        protected override void Dispose(bool disposing) {
        }

        private static PathGeometry GetPathFromPolygon(PointF[] polygon, Factory factory) {
            if (polygon == null) {
                throw new ArgumentNullException(nameof(polygon));
            }
            if (polygon.Length < 3) {
                throw new ArgumentException("A polygon should have at least 3 points.");
            }
            var path = new PathGeometry(factory);
            using (var s = path.Open()) {
                s.BeginFigure(new RawVector2(polygon[0].X, polygon[0].Y), FigureBegin.Filled);
                for (var i = 1; i < polygon.Length; ++i) {
                    s.AddLine(new RawVector2(polygon[i].X, polygon[i].Y));
                }
                s.EndFigure(FigureEnd.Closed);
                s.Close();
            }
            return path;
        }

    }
}
