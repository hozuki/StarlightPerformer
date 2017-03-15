using SharpDX.Direct2D1;
using StarlightPerformer.Core;
using StarlightPerformer.Stage;

namespace StarlightPerformer.Elements {
    public sealed class AvatarLayer : Element {

        public override void Initialize() {
            base.Initialize();
        }

        public override void OnGotContext(RenderContext context) {
            base.OnGotContext(context);
            _avatarTop = D2DHelper.LoadBitmap(TopAvatarsFileName, context.RenderTarget);
            _avatarBottom = D2DHelper.LoadBitmap(BottomAvatarsFileName, context.RenderTarget);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _avatarTop?.Dispose();
                _avatarBottom?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void DrawInternal(GameTime gameTime, RenderContext context) {
            base.DrawInternal(gameTime, context);
            const int typeIndex = 3;

            var stageSize = context.ClientSize;
            var x = (stageSize.Width - ImageWidth) / 2;
            var y = stageSize.Height * AvatarCentralYRatio - ImageHeight / 2;
            var srcX = 0;
            var srcY = typeIndex * ImageHeight;
            var w = ImageWidth;
            var h = ImageHeight;

            context.DrawImage(_avatarBottom, x, y, w, h, srcX, srcY, w, h);
            context.DrawImage(_avatarTop, x, y, w, h, srcX, srcY, w, h);
        }

        private static readonly string TopAvatarsFileName = "Resources/images/ui/ring_lower.png";
        private static readonly string BottomAvatarsFileName = "Resources/images/ui/ring_upper.png";

        private static readonly float ImageWidth = 1008;
        private static readonly float ImageHeight = 142;

        private static readonly float AvatarCentralYRatio = 0.828125f;

        private Bitmap _avatarTop;
        private Bitmap _avatarBottom;

    }
}
