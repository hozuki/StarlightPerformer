using System.Diagnostics;
using System.Drawing;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using PixelFormat = SharpDX.WIC.PixelFormat;

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

        public static BitmapSource LoadBitmapAsWic(string fileName) {
            using (var factory = new ImagingFactory()) {
                using (var decoder = new BitmapDecoder(factory, fileName, DecodeOptions.CacheOnDemand)) {
                    var converter = new FormatConverter(factory);
                    converter.Initialize(decoder.GetFrame(0), PixelFormat.Format32bppPBGRA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);
                    return converter;
                }
            }
        }

        public static Bitmap LoadBitmap(string fileName, RenderTarget target) {
            using (var bmp = LoadBitmapAsWic(fileName)) {
                return Bitmap.FromWicBitmap(target, bmp);
            }
        }

    }
}
