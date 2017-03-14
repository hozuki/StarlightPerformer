namespace StarlightPerformer {
    partial class FStartup {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.txtScoreFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowseScore = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMusicFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowseMusic = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Score:";
            // 
            // txtScoreFilePath
            // 
            this.txtScoreFilePath.Location = new System.Drawing.Point(59, 26);
            this.txtScoreFilePath.Name = "txtScoreFilePath";
            this.txtScoreFilePath.Size = new System.Drawing.Size(283, 21);
            this.txtScoreFilePath.TabIndex = 1;
            // 
            // btnBrowseScore
            // 
            this.btnBrowseScore.Location = new System.Drawing.Point(348, 22);
            this.btnBrowseScore.Name = "btnBrowseScore";
            this.btnBrowseScore.Size = new System.Drawing.Size(32, 26);
            this.btnBrowseScore.TabIndex = 2;
            this.btnBrowseScore.Text = "...";
            this.btnBrowseScore.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(315, 197);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(77, 33);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "&Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "Music:";
            // 
            // txtMusicFilePath
            // 
            this.txtMusicFilePath.Location = new System.Drawing.Point(59, 58);
            this.txtMusicFilePath.Name = "txtMusicFilePath";
            this.txtMusicFilePath.Size = new System.Drawing.Size(283, 21);
            this.txtMusicFilePath.TabIndex = 5;
            // 
            // btnBrowseMusic
            // 
            this.btnBrowseMusic.Location = new System.Drawing.Point(348, 54);
            this.btnBrowseMusic.Name = "btnBrowseMusic";
            this.btnBrowseMusic.Size = new System.Drawing.Size(32, 26);
            this.btnBrowseMusic.TabIndex = 6;
            this.btnBrowseMusic.Text = "...";
            this.btnBrowseMusic.UseVisualStyleBackColor = true;
            // 
            // FStartup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 242);
            this.Controls.Add(this.btnBrowseMusic);
            this.Controls.Add(this.txtMusicFilePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnBrowseScore);
            this.Controls.Add(this.txtScoreFilePath);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FStartup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Starlight Performer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtScoreFilePath;
        private System.Windows.Forms.Button btnBrowseScore;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMusicFilePath;
        private System.Windows.Forms.Button btnBrowseMusic;
    }
}