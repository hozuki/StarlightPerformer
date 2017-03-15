namespace StarlightPerformer.Core {
    public static class MathUtils {

        public static float Clamp(this float v, float min, float max) {
            return v < min ? min : (v > max ? max : v);
        }

    }
}
