namespace ChartControlSample
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LeftPercControl = new System.Windows.Forms.VScrollBar();
            this.RightPercControl = new System.Windows.Forms.VScrollBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelVal1 = new System.Windows.Forms.Label();
            this.labelVal2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LeftPercControl
            // 
            this.LeftPercControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LeftPercControl.Location = new System.Drawing.Point(36, 27);
            this.LeftPercControl.Name = "LeftPercControl";
            this.LeftPercControl.Size = new System.Drawing.Size(30, 335);
            this.LeftPercControl.TabIndex = 0;
            this.LeftPercControl.Value = 100;
            this.LeftPercControl.ValueChanged += new System.EventHandler(this.OnLeftValueChanged);
            // 
            // RightPercControl
            // 
            this.RightPercControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RightPercControl.Location = new System.Drawing.Point(171, 27);
            this.RightPercControl.Name = "RightPercControl";
            this.RightPercControl.Size = new System.Drawing.Size(30, 335);
            this.RightPercControl.TabIndex = 1;
            this.RightPercControl.Value = 100;
            this.RightPercControl.ValueChanged += new System.EventHandler(this.OnRightValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.Yellow;
            this.label1.Location = new System.Drawing.Point(10, 373);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 29);
            this.label1.TabIndex = 2;
            this.label1.Text = "Party A";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.label2.ForeColor = System.Drawing.Color.Yellow;
            this.label2.Location = new System.Drawing.Point(142, 373);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 29);
            this.label2.TabIndex = 3;
            this.label2.Text = "Party B";
            // 
            // labelVal1
            // 
            this.labelVal1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelVal1.AutoSize = true;
            this.labelVal1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelVal1.ForeColor = System.Drawing.Color.White;
            this.labelVal1.Location = new System.Drawing.Point(35, 402);
            this.labelVal1.Name = "labelVal1";
            this.labelVal1.Size = new System.Drawing.Size(41, 25);
            this.labelVal1.TabIndex = 4;
            this.labelVal1.Text = "0%";
            // 
            // labelVal2
            // 
            this.labelVal2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVal2.AutoSize = true;
            this.labelVal2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelVal2.ForeColor = System.Drawing.Color.White;
            this.labelVal2.Location = new System.Drawing.Point(168, 402);
            this.labelVal2.Name = "labelVal2";
            this.labelVal2.Size = new System.Drawing.Size(41, 25);
            this.labelVal2.TabIndex = 5;
            this.labelVal2.Text = "0%";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(241, 437);
            this.Controls.Add(this.labelVal2);
            this.Controls.Add(this.labelVal1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RightPercControl);
            this.Controls.Add(this.LeftPercControl);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainForm";
            this.Text = "Manual Type Controller";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.VScrollBar LeftPercControl;
        private System.Windows.Forms.VScrollBar RightPercControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelVal1;
        private System.Windows.Forms.Label labelVal2;

    }
}

