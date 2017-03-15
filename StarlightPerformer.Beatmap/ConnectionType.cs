using System;

namespace StarlightPerformer.Beatmap {
    [Flags]
    public enum ConnectionType {

        None = 0,
        Hold = 1,
        Flick = 2,
        Slide = 3,
        RawMask = 0x0f,
        Sync = 0x10,
        SyncMask = 0xf0

    }
}
