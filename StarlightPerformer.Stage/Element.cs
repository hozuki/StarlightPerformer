using StarlightPerformer.Core;

namespace StarlightPerformer.Stage {
    public abstract class Element : DisposableBase {

        public bool Enabled { get; set; } = true;

        public bool Visible { get; set; } = true;

        public virtual void Initialize() {
        }

        public virtual void OnLostContext(RenderContext context) {
            Dispose(true);
        }

        public virtual void OnGotContext(RenderContext context) {
        }

        public void Update(GameTime gameTime) {
            if (Enabled) {
                UpdateInternal(gameTime);
            }
        }

        public void Draw(GameTime gameTime, RenderContext context) {
            if (Visible) {
                DrawInternal(gameTime, context);
            }
        }

        protected virtual void UpdateInternal(GameTime gameTime) {
        }

        protected virtual void DrawInternal(GameTime gameTime, RenderContext context) {
        }

    }
}
