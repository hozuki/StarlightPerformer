using System.Collections.Generic;
using StarlightPerformer.Beatmap;
using StarlightPerformer.Core;
using StarlightPerformer.Elements.Internal;
using StarlightPerformer.Stage;

namespace StarlightPerformer.Elements {
    internal sealed class NoteLeavingSoundEmitter : Element {

        public NoteLeavingSoundEmitter(Score score) {
            _statusList = new Dictionary<Note, OnStageStatus>(score.Notes.Count);
            foreach (var note in score.Notes) {
                _statusList[note] = OnStageStatus.Upcoming;
            }
        }

        protected override void UpdateInternal(GameTime gameTime) {
            base.UpdateInternal(gameTime);

            var dict = _statusList;
            var now = gameTime.Total.TotalSeconds;
            foreach (var s in dict) {
                var oldState = s.Value;
                var newState = NotesLayerUtils.GetNoteOnStageStatus(s.Key, now);
                if (oldState == newState) {
                    continue;
                }
                if (newState == OnStageStatus.Passed && oldState == OnStageStatus.OnStage) {
                    // TODO: Play sound.
                }
                dict[s.Key] = newState;
            }
        }

        private readonly Dictionary<Note, OnStageStatus> _statusList;

    }
}
