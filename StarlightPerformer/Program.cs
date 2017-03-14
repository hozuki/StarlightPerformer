using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StarlightPerformer {
    internal static class Program {

        public static IReadOnlyList<string> Arguments { get; private set; }

        public static StartupOptions Options { get; internal set; }

        [STAThread]
        private static void Main(string[] args) {
            Arguments = args;
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            using (var fStartup = new FStartup()) {
                var r = fStartup.ShowDialog();
                if (r != DialogResult.OK) {
                    return;
                }
            }
            using (var game = new StarlightStage(Options)) {
                game.Run(args);
            }
        }

    }
}
