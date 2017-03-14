using System;

namespace StarlightPerformer.Core {
    public sealed class GameTime {

        public GameTime(TimeSpan delta, TimeSpan total) {
            Delta = delta;
            Total = total;
        }

        public TimeSpan Delta { get; }

        public TimeSpan Total { get; }

    }
}
