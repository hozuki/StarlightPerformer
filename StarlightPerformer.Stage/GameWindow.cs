using System;
using System.Drawing;
using System.Windows.Forms;

namespace StarlightPerformer.Stage {
    public partial class GameWindow : Form {

        public GameWindow(Game game) {
            Game = game;
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, false);
            CheckForIllegalCrossThreadCalls = false;
        }

        public Game Game { get; }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(DefaultClientWidth, DefaultClientHeight);
            var screenRect = Screen.FromControl(this).WorkingArea;
            var size = Size;
            Location = new Point((screenRect.Width - size.Width) / 2, (screenRect.Height - size.Height) / 2);
        }
        
        protected override void OnClosed(EventArgs e) {
            Game.ContinueLogic = false;
            base.OnClosed(e);
        }

        public static readonly int DefaultClientWidth = 1280;

        public static readonly int DefaultClientHeight = 720;

    }
}
