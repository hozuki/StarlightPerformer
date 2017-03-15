using System.Collections.Generic;
using StarlightPerformer.Core;

namespace StarlightPerformer.Stage {
    public abstract class Element : DisposableBase {

        public bool Enabled { get; set; } = true;

        public bool Visible { get; set; } = true;

        public List<Element> Elements { get; } = new List<Element>();

        public virtual void Initialize() {
            foreach (var element in Elements) {
                element.Initialize();
            }
        }

        public virtual void OnLostContext(RenderContext context) {
            Dispose(true);
            foreach (var element in Elements) {
                element.OnLostContext(context);
            }
        }

        public virtual void OnGotContext(RenderContext context) {
            foreach (var element in Elements) {
                element.OnGotContext(context);
            }
        }

        public void Update(GameTime gameTime) {
            if (Enabled) {
                UpdateInternal(gameTime);
                foreach (var element in Elements) {
                    element.Update(gameTime);
                }
            }
        }

        public void Draw(GameTime gameTime, RenderContext context) {
            if (Visible) {
                DrawInternal(gameTime, context);
                foreach (var element in Elements) {
                    element.Draw(gameTime, context);
                }
            }
        }

        protected virtual void UpdateInternal(GameTime gameTime) {
        }

        protected virtual void DrawInternal(GameTime gameTime, RenderContext context) {
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                foreach (var element in Elements) {
                    element.Dispose();
                }
            }
        }

    }
}
