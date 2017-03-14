namespace StarlightPerformer.Beatmap {
    public sealed class Note {

        public int ID { get; set; }

        public NoteType Type { get; set; }

        public double HitTiming { get; set; }

        public NotePosition StartPosition { get; set; }

        public NotePosition FinishPosition { get; set; }

        public NoteFlickType FlickType { get; set; }

        public bool IsSync { get; set; }

        public int GroupID { get; set; }

        /* Helpers */

        public Note SyncPair { get; internal set; }

        public Note NextFlickOrSlide { get; internal set; }

        public Note PrevFlickOrSlide { get; internal set; }

        public Note HoldPair { get; internal set; }

        public bool IsTap => Type == NoteType.TapOrFlick && FlickType == NoteFlickType.None;

        public bool IsFlick => FlickType != NoteFlickType.None;

        public bool IsFlickStart => IsFlick && HasNextFlickOrSlide && !HasPrevFlickOrSlide;

        public bool IsFlickEnd => IsFlick && HasPrevFlickOrSlide && !HasNextFlickOrSlide;

        public bool IsFlickMidway => IsFlick && HasPrevFlickOrSlide && HasNextFlickOrSlide;

        public bool IsHoldStart => Type == NoteType.Hold;

        public bool IsHoldEnd => HasHold && IsTap;

        public bool IsHold => IsHoldStart || IsHoldEnd;

        public bool HasHold => HoldPair != null;

        public bool IsSlide => Type == NoteType.Slide && FlickType == NoteFlickType.None;

        public bool IsSlideStart => IsSlide && HasNextFlickOrSlide && !HasPrevFlickOrSlide;

        public bool IsSlideEnd => IsSlide && HasPrevFlickOrSlide && !HasNextFlickOrSlide;

        public bool IsSlideMidway => IsSlide && HasPrevFlickOrSlide && HasNextFlickOrSlide;

        public bool HasPrevFlickOrSlide => PrevFlickOrSlide != null;

        public bool HasNextFlickOrSlide => NextFlickOrSlide != null;

    }
}
