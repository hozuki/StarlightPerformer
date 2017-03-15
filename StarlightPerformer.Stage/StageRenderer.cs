using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using StarlightPerformer.Core;

namespace StarlightPerformer.Stage {
    public sealed class StageRenderer : DisposableBase {

        public StageRenderer(Game game, Control control) {
            Game = game;
            Control = control;
            Initialize();
        }

        public Control Control { get; }

        public Game Game { get; }

        public Color ClearColor { get; set; } = Color.Black;

        public void Draw(IList<Element> elements, GameTime gameTime) {
            var context = _renderContext;
            lock (_sizeLock) {
                if (_isSizeChanged) {
                    foreach (var element in elements) {
                        element.OnLostContext(context);
                    }
                    _renderTarget.Dispose();
                    _hwndRenderTargetProperties.PixelSize = _newSize;
                    _renderTarget = new WindowRenderTarget(_factory, _renderTargetProperties, _hwndRenderTargetProperties);
                    context = _renderContext = new RenderContext(this, _renderTarget, new Size(_newSize.Width, _newSize.Height));
                    foreach (var element in elements) {
                        element.OnGotContext(context);
                    }
                    _isSizeChanged = false;
                }
            }

            context.BeginDraw();
            foreach (var element in elements) {
                element.Draw(gameTime, context);
            }
            context.EndDraw();
        }

        protected override void Dispose(bool disposing) {
            var control = Control;
            control.ClientSizeChanged -= ControlOnClientSizeChanged;

            _renderTarget.Dispose();
            _renderTarget = null;
            _factory.Dispose();
            _factory = null;
        }

        private void Initialize() {
            var control = Control;
            var factory = _factory = new Factory();

            _hwndRenderTargetProperties.Hwnd = control.Handle;
            Size clientSize = control.ClientSize;
            _hwndRenderTargetProperties.PixelSize = new Size2(clientSize.Width, clientSize.Height);
            _renderTarget = new WindowRenderTarget(factory, _renderTargetProperties, _hwndRenderTargetProperties);

            var context = _renderContext = new RenderContext(this, _renderTarget, clientSize);
            foreach (var element in Game.Elements) {
                element.OnGotContext(context);
            }

            control.ClientSizeChanged += ControlOnClientSizeChanged;
        }

        private void ControlOnClientSizeChanged(object sender, EventArgs eventArgs) {
            lock (_sizeLock) {
                var control = (Control)sender;
                _isSizeChanged = true;
                var clientSize = control.ClientSize;
                _newSize = new Size2(clientSize.Width, clientSize.Height);
            }
        }


        private readonly RenderTargetProperties _renderTargetProperties = new RenderTargetProperties(new PixelFormat());
        private HwndRenderTargetProperties _hwndRenderTargetProperties;
        private Factory _factory;
        private RenderTarget _renderTarget;
        private RenderContext _renderContext;

        private bool _isSizeChanged;
        private Size2 _newSize;
        private readonly object _sizeLock = new object();

    }
}
