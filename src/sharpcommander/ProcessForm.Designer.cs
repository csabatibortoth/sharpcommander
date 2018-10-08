namespace sharpcommander
{
    partial class ProcessForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.feladatLbl = new System.Windows.Forms.Label();
            this.taskProg = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.dbLbl = new System.Windows.Forms.Label();
            this.overdbLbl = new System.Windows.Forms.Label();
            this.overProg = new System.Windows.Forms.ProgressBar();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Feladat:";
            // 
            // feladatLbl
            // 
            this.feladatLbl.AutoSize = true;
            this.feladatLbl.Location = new System.Drawing.Point(118, 13);
            this.feladatLbl.Name = "feladatLbl";
            this.feladatLbl.Size = new System.Drawing.Size(35, 13);
            this.feladatLbl.TabIndex = 1;
            this.feladatLbl.Text = "label2";
            // 
            // taskProg
            // 
            this.taskProg.Location = new System.Drawing.Point(16, 29);
            this.taskProg.Name = "taskProg";
            this.taskProg.Size = new System.Drawing.Size(488, 23);
            this.taskProg.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Teljes Folyamat:";
            // 
            // dbLbl
            // 
            this.dbLbl.AutoSize = true;
            this.dbLbl.Location = new System.Drawing.Point(469, 13);
            this.dbLbl.Name = "dbLbl";
            this.dbLbl.Size = new System.Drawing.Size(35, 13);
            this.dbLbl.TabIndex = 4;
            this.dbLbl.Text = "label3";
            // 
            // overdbLbl
            // 
            this.overdbLbl.AutoSize = true;
            this.overdbLbl.Location = new System.Drawing.Point(469, 59);
            this.overdbLbl.Name = "overdbLbl";
            this.overdbLbl.Size = new System.Drawing.Size(35, 13);
            this.overdbLbl.TabIndex = 5;
            this.overdbLbl.Text = "label3";
            // 
            // overProg
            // 
            this.overProg.Location = new System.Drawing.Point(16, 75);
            this.overProg.Name = "overProg";
            this.overProg.Size = new System.Drawing.Size(488, 23);
            this.overProg.TabIndex = 6;
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(219, 105);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 7;
            this.cancelBtn.Text = "Mégse";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(375, 105);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // ProcessForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(520, 138);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.overProg);
            this.Controls.Add(this.overdbLbl);
            this.Controls.Add(this.dbLbl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.taskProg);
            this.Controls.Add(this.feladatLbl);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "ProcessForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ProcessForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label feladatLbl;
        private System.Windows.Forms.ProgressBar taskProg;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label dbLbl;
        private System.Windows.Forms.Label overdbLbl;
        private System.Windows.Forms.ProgressBar overProg;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button button1;
    }
}