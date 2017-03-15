using System;
using System.Drawing;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using StarlightPerformer.Beatmap;
using StarlightPerformer.Stage;
using Brush = SharpDX.Direct2D1.Brush;

namespace StarlightPerformer.Elements.Internal {
    internal sealed class RibbonMesh {

        public RibbonMesh(RenderContext context, Note startNote, Note endNote, double now, ConnectionType connectionType) {
            RenderContext = context;
            StartNote = startNote;
            EndNote = endNote;
            Now = now;
            ConnectionType = connectionType;

            if ((connectionType & ConnectionType.RawMask) == ConnectionType.Slide) {
                InitializeSlide();
            } else {
                Initialize();
            }
            BuildTriangles();
        }

        public RenderContext RenderContext { get; }

        public Note StartNote { get; }

        public Note EndNote { get; }

        public bool IsSlideRibbon { get; }

        public ConnectionType ConnectionType { get; }

        public void Fill(Brush brush) {
            var context = RenderContext;
            var target = context.RenderTarget;
            using (var mesh = new Mesh(target, Triangles)) {
                // https://msdn.microsoft.com/en-us/library/dd371939.aspx
                target.AntialiasMode = AntialiasMode.Aliased;
                context.FillMesh(mesh, brush);
                target.AntialiasMode = AntialiasMode.PerPrimitive;
            }
        }

        private double Now { get; }

        private void Initialize() {
            var startTiming = StartNote.HitTiming;
            var endTiming = EndNote.HitTiming;
            var finishPosition = EndNote.FinishPosition;
            var now = Now;
            var context = RenderContext;

            var xs = new float[JointCount];
            var ys = new float[JointCount];
            var rs = new float[JointCount];

            var rawConnection = ConnectionType & ConnectionType.RawMask;
            switch (rawConnection) {
                case ConnectionType.Hold:
                    for (var i = 0; i < JointCount; ++i) {
                        var timing = (endTiming - startTiming) / (JointCount - 1) * i + startTiming;
                        xs[i] = NotesLayerUtils.GetNoteXPosition(context, now, timing, finishPosition, true, true);
                        ys[i] = NotesLayerUtils.GetNoteYPosition(context, now, timing, true, true);
                        rs[i] = NotesLayerUtils.GetNoteRadius(now, timing);
                    }
                    break;
                case ConnectionType.Flick:
                    var x1 = NotesLayerUtils.GetNoteXPosition(context, now, StartNote, true, true);
                    var x2 = NotesLayerUtils.GetNoteXPosition(context, now, EndNote, true, true);
                    var y1 = NotesLayerUtils.GetNoteYPosition(context, now, StartNote, true, true);
                    var y2 = NotesLayerUtils.GetNoteYPosition(context, now, EndNote, true, true);
                    var r1 = NotesLayerUtils.GetNoteRadius(now, StartNote);
                    var r2 = NotesLayerUtils.GetNoteRadius(now, EndNote);
                    for (var i = 0; i < JointCount; ++i) {
                        var t = (float)i / (JointCount - 1);
                        xs[i] = D2DHelper.Lerp(x1, x2, t);
                        ys[i] = D2DHelper.Lerp(y1, y2, t);
                        rs[i] = D2DHelper.Lerp(r1, r2, t);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rawConnection));
            }

            for (var i = 0; i < JointCount; ++i) {
                var x = xs[i];
                var y = ys[i];
                var r = rs[i];
                RawVector2 vertex1, vertex2;
                switch (rawConnection) {
                    case ConnectionType.Hold:
                        vertex1 = new RawVector2(x - r, y);
                        vertex2 = new RawVector2(x + r, y);
                        break;
                    case ConnectionType.Flick:
                        float ydif, xdif;
                        if (i == JointCount - 1) {
                            ydif = y - ys[i - 1];
                            xdif = x - xs[i - 1];
                        } else {
                            ydif = ys[i + 1] - y;
                            xdif = xs[i + 1] - x;
                        }
                        var rad = (float)Math.Atan2(ydif, xdif);
                        var cos = (float)Math.Cos(rad);
                        var sin = (float)Math.Sin(rad);
                        vertex1 = new RawVector2(x - r * sin, y - r * cos);
                        vertex2 = new RawVector2(x + r * sin, y + r * cos);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(rawConnection));
                }
                Vertices[i * 2] = vertex1;
                Vertices[i * 2 + 1] = vertex2;
            }
        }

        private void InitializeSlide() {
            throw new NotImplementedException();
        }

        private void BuildTriangles() {
            for (var i = 0; i < JointCount - 1; ++i) {
                var t1 = new Triangle();
                var t2 = new Triangle();
                RawVector2 v1 = Vertices[i * 2], v2 = Vertices[i * 2 + 1], v3 = Vertices[i * 2 + 3], v4 = Vertices[i * 2 + 2];
                t1.Point1 = v1;
                t1.Point2 = v2;
                t1.Point3 = v4;
                t2.Point1 = v2;
                t2.Point2 = v3;
                t2.Point3 = v4;
                Triangles[i * 2] = t1;
                Triangles[i * 2 + 1] = t2;
            }
        }

        private static Color GetRibbonVertexColor(float f, SongColor songColor, bool simple) {
            var n1 = (int)f;
            var n2 = n1 + 1;
            f %= 1;
            Color[] colors;
            if (simple) {
                colors = SimpleWhiteColors;
            } else {
                switch (songColor) {
                    case SongColor.All:
                        colors = RainbowColors;
                        break;
                    case SongColor.Cute:
                        colors = CuteColors;
                        break;
                    case SongColor.Cool:
                        colors = CoolColors;
                        break;
                    case SongColor.Passion:
                        colors = PassionColors;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(songColor));
                }
            }
            n1 %= colors.Length;
            n2 %= colors.Length;
            var color1 = colors[n1];
            var color2 = colors[n2];
            return D2DHelper.Lerp(color1, color2, f);
        }

        private static readonly Color[] RainbowColors = {
            D2DHelper.GetFloatColor(0.8f, 1f, 0.65f, 0.65f),
            D2DHelper.GetFloatColor(0.8f, 0.65f, 1f, 0.65f),
            D2DHelper.GetFloatColor(0.8f, 0.65f, 0.65f, 1f)
        };

        private static readonly Color[] CoolColors = {
            D2DHelper.GetFloatColor(0.8f, 0.211764708f, 0.929411769f, 0.992156863f),
            D2DHelper.GetFloatColor(0.8f, 0.164705887f, 0.478431374f, 1f)
        };

        private static readonly Color[] CuteColors = {
            D2DHelper.GetFloatColor(0.8f, 0.921568632f, 0.6784314f, 0.694117665f),
            D2DHelper.GetFloatColor(0.8f, 1f, 0.3019608f, 0.533333361f)
        };

        private static readonly Color[] PassionColors = {
            D2DHelper.GetFloatColor(0.8f, 1f, 0.827451f, 0.129411772f),
            D2DHelper.GetFloatColor(0.8f, 0.854901969f, 0.5647059f, 0f)
        };

        private static readonly Color[] SimpleWhiteColors = {
            D2DHelper.GetFloatColor(0.8f, 0.784313738f, 0.784313738f, 0.784313738f),
            D2DHelper.GetFloatColor(0.8f, 1f, 1f, 1f)
        };

        private const int JointCount = 48;

        private const float Unk = -160;

        private readonly RawVector2[] Vertices = new RawVector2[JointCount * 2];
        private Triangle[] Triangles = new Triangle[2 * (JointCount - 1)];

    }
}
