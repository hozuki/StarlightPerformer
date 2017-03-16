namespace StarlightPerformer {
    internal static class Definitions {

        public static readonly float NoteImageCellWidth = 154;
        public static readonly float NoteImageCellHeight = 110;
        public static readonly float NoteImageAspectRatio = NoteImageCellWidth / NoteImageCellHeight;

        public static readonly float AvatarFrameImageCellWidth = 1008;
        public static readonly float AvatarFrameImageCellHeight = 142;

        public static float FutureTimeWindow = 1f;
        public static readonly float PastTimeWindow = 0.2f;
        public static readonly float AvatarCircleDiameter = 110;
        public static readonly float AvatarCircleRadius = AvatarCircleDiameter / 2;
        public static readonly float[] AvatarCenterXStartPositions = { 0.3f, 0.4f, 0.5f, 0.6f, 0.7f };
        public static readonly float[] AvatarCenterXEndPositions = { 0.18f, 0.34f, 0.5f, 0.66f, 0.82f };
        public static readonly float BaseLineYPosition = 0.828125f;
        // Then we know the bottom is <BaseLineYPosition + (PastWindow / FutureWindow) * (BaseLineYPosition - Ceiling))>.
        public static readonly float FutureNoteCeiling = 0.21875f;

        public static readonly string NotesBitmapFilePath = "Resources/images/notes/notes.png";
        public static readonly string TopAvatarsFileName = "Resources/images/ui/ring_lower.png";
        public static readonly string BottomAvatarsFileName = "Resources/images/ui/ring_upper.png";

    }
}
