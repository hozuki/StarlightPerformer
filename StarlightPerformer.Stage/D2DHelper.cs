using System.Diagnostics;
using System.Drawing;
using SharpDX.Mathematics.Interop;

namespace StarlightPerformer.Stage {
    public static class D2DHelper {

        [DebuggerStepThrough]
        public static RawColor4 ColorToRC4(this Color color) {
            var a = (float)color.A / byte.MaxValue;
            var r = (float)color.R / byte.MaxValue;
            var g = (float)color.G / byte.MaxValue;
            var b = (float)color.B / byte.MaxValue;
            return new RawColor4(r, g, b, a);
        }

        [DebuggerStepThrough]
        public static RawColor3 ColorToRC3(this Color color) {
            var r = (float)color.R / byte.MaxValue;
            var g = (float)color.G / byte.MaxValue;
            var b = (float)color.B / byte.MaxValue;
            return new RawColor3(r, g, b);
        }

    }
}
