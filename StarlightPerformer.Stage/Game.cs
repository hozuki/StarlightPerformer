using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using StarlightPerformer.Core;

namespace StarlightPerformer.Stage {
    public class Game : DisposableBase {

        public void Update(GameTime gameTime) {
            foreach (var element in Elements) {
                element.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime) {
            Renderer.Draw(Elements, gameTime);
        }

        public void Run() => Run(new string[0]);

        public virtual void Initialize() {
            ScoreMusic = new ScoreMusicPlayer();
            foreach (var element in Elements) {
                element.Initialize();
            }
        }

        public void Run(string[] args) {
            Application.EnableVisualStyles();
            using (var window = new GameWindow(this)) {
                Window = window;
                Initialize();
                using (Renderer = new StageRenderer(this, window)) {
                    _renderThread = new Thread(RenderThreadProc);
                    _renderThread.IsBackground = true;
                    _exitingEvent = new ManualResetEvent(false);
                    _renderThread.Start(window);
                    OnStart();

                    window.ShowDialog();

                    _exitingEvent.WaitOne();
                    _exitingEvent.Dispose();
                }
            }
        }

        public List<Element> Elements { get; } = new List<Element>();

        public StageRenderer Renderer { get; private set; }

        public GameTime Time { get; private set; }

        public GameWindow Window { get; private set; }

        public ScoreMusicPlayer ScoreMusic { get; private set; }

        protected virtual void OnStart() {
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                foreach (var element in Elements) {
                    element.Dispose();
                }
                Renderer.Dispose();
                ScoreMusic.Dispose();
            }
        }

        private void RenderThreadProc(object param) {
            var startTick = PerformanceCounter.GetCurrent();
            var lastTick = startTick;
            while (ContinueLogic) {
                var nowTick = PerformanceCounter.GetCurrent();
                var delta = PerformanceCounter.GetDuration(lastTick, nowTick);
                var total = PerformanceCounter.GetDuration(startTick, nowTick);
                var gameTime = new GameTime(TimeSpan.FromMilliseconds(delta), TimeSpan.FromMilliseconds(total));
                Time = gameTime;
                Update(gameTime);
                Draw(gameTime);
            }
            _exitingEvent?.Set();
        }

        internal volatile bool ContinueLogic = true;
        private Thread _renderThread;
        private ManualResetEvent _exitingEvent;

    }
}
