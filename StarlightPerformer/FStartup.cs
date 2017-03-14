using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StarlightPerformer {
    public partial class FStartup : Form {

        public FStartup() {
            InitializeComponent();
            RegisterEventHandlers();
        }

        ~FStartup() {
            UnregisterEventHandlers();
        }

        private void UnregisterEventHandlers() {
            btnBrowseMusic.Click -= BtnBrowseMusic_Click;
            btnBrowseScore.Click -= BtnBrowseScore_Click;
            btnStart.Click -= BtnStart_Click;
            Load -= FStartup_Load;
        }

        private void RegisterEventHandlers() {
            btnBrowseMusic.Click += BtnBrowseMusic_Click;
            btnBrowseScore.Click += BtnBrowseScore_Click;
            btnStart.Click += BtnStart_Click;
            Load += FStartup_Load;
        }

        private void BtnBrowseMusic_Click(object sender, EventArgs e) {
            var ofd = openFileDialog;
            ofd.CheckFileExists = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = "Wave Audio (*.wav)|*.wav";
            var r = ofd.ShowDialog(this);
            if (r != DialogResult.OK) {
                return;
            }
            txtMusicFilePath.Text = ofd.FileName;
        }

        private void BtnBrowseScore_Click(object sender, EventArgs e) {
            var ofd = openFileDialog;
            ofd.CheckFileExists = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = "CGSS Scores (*.csv;*.bdb)|*.csv;*.bdb|Score Bundle (*.bdb)|*.bdb|Single Score (*.csv)|*.csv";
            var r = ofd.ShowDialog(this);
            if (r != DialogResult.OK) {
                return;
            }
            txtScoreFilePath.Text = ofd.FileName;
        }

        private void BtnStart_Click(object sender, EventArgs e) {
            var messages = ValidateFields();
            if (messages == null || messages.Length == 0) {
                ApplyFields();
                DialogResult = DialogResult.OK;
                return;
            }
            const string errStart = "Sorry, you have to rethink about some fields:";
            var newLine = Environment.NewLine;
            var err = messages.Aggregate(errStart, (v, s) => v == null ? s : v + newLine + s);
            MessageBox.Show(this, err, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void FStartup_Load(object sender, EventArgs e) {
            LoadFields();
        }

        private string[] ValidateFields() {
            var errors = new List<string>();
            if (string.IsNullOrEmpty(txtScoreFilePath.Text) || !File.Exists(txtScoreFilePath.Text)) {
                errors.Add($"Score file '{txtScoreFilePath.Text}' does not exist.");
            }
            if (string.IsNullOrEmpty(txtMusicFilePath.Text) || !File.Exists(txtMusicFilePath.Text)) {
                errors.Add($"Music file '{txtMusicFilePath.Text}' does not exist.");
            }
            return errors.ToArray();
        }

        private void ApplyFields() {
            var options = new StartupOptions();
            options.ScoreFilePath = txtScoreFilePath.Text;
            options.MusicFilePath = txtMusicFilePath.Text;
            Program.Options = options;
        }

        private void LoadFields() {
            var options = Program.Options;
            if (options == null) {
                return;
            }
            txtScoreFilePath.Text = options.ScoreFilePath;
        }

    }
}
