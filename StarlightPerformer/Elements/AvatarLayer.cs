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
            _avatarTop = D2DHelper.LoadBitmap(Definitions.TopAvatarsFileName, context.RenderTarget);
            _avatarBottom = D2DHelper.LoadBitmap(Definitions.BottomAvatarsFileName, context.RenderTarget);
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
            var x = (stageSize.Width - Definitions.AvatarFrameImageCellWidth) / 2;
            var y = stageSize.Height * Definitions.BaseLineYPosition - Definitions.AvatarFrameImageCellHeight / 2;
            var srcX = 0;
            var srcY = typeIndex * Definitions.AvatarFrameImageCellHeight;
            var w = Definitions.AvatarFrameImageCellWidth;
            var h = Definitions.AvatarFrameImageCellHeight;

            context.DrawImage(_avatarBottom, x, y, w, h, srcX, srcY, w, h);
            context.DrawImage(_avatarTop, x, y, w, h, srcX, srcY, w, h);
        }



        private Bitmap _avatarTop;
        private Bitmap _avatarBottom;

    }
}
