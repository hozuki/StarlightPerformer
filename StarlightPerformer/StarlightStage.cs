using StarlightPerformer.Beatmap;
using StarlightPerformer.Elements;
using StarlightPerformer.Stage;

namespace StarlightPerformer {
    public sealed class StarlightStage : Game {

        public StarlightStage(StartupOptions options) {
            Options = options;
        }

        public StartupOptions Options { get; }

        public override void Initialize() {
            var options = Options;

            Elements.Add(new AvatarLayer());

            var score = Score.FromFile(options.ScoreFilePath);
            Elements.Add(new NotesLayer(score));
            Elements.Add(new NoteLeavingSoundEmitter(score));

            base.Initialize();
        }

        protected override void OnStart() {
            base.OnStart();
            ScoreMusic.PlayFile(Options.MusicFilePath);
        }

    }
}
