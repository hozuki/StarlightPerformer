using SharpDX.Direct2D1;
using StarlightPerformer.Core;

namespace StarlightPerformer.Stage {
    public sealed class LinearGradientBrushEx : DisposableBase {

        public LinearGradientBrushEx(RenderTarget renderTarget, LinearGradientBrushProperties properties, GradientStopCollection collection) {
            _brush = new LinearGradientBrush(renderTarget, properties, collection);
            _collection = collection;
        }

        public LinearGradientBrush Brush => _brush;

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _brush.Dispose();
                _collection.Dispose();
            }
        }

        private readonly LinearGradientBrush _brush;
        private readonly GradientStopCollection _collection;

    }
}
