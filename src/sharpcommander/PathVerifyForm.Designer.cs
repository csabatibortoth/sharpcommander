namespace sharpcommander
{
    partial class PathVerifyForm
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
            this.resourceLBL = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.targetLBL = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.acceptBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // resourceLBL
            // 
            this.resourceLBL.AutoSize = true;
            this.resourceLBL.Location = new System.Drawing.Point(13, 13);
            this.resourceLBL.Name = "resourceLBL";
            this.resourceLBL.Size = new System.Drawing.Size(39, 13);
            this.resourceLBL.TabIndex = 0;
            this.resourceLBL.Text = "Forrás:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(16, 30);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(413, 20);
            this.textBox1.TabIndex = 1;
            // 
            // targetLBL
            // 
            this.targetLBL.AutoSize = true;
            this.targetLBL.Location = new System.Drawing.Point(16, 57);
            this.targetLBL.Name = "targetLBL";
            this.targetLBL.Size = new System.Drawing.Size(25, 13);
            this.targetLBL.TabIndex = 2;
            this.targetLBL.Text = "Cél:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(16, 74);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(413, 20);
            this.textBox2.TabIndex = 3;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // acceptBtn
            // 
            this.acceptBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.acceptBtn.Location = new System.Drawing.Point(273, 103);
            this.acceptBtn.Name = "acceptBtn";
            this.acceptBtn.Size = new System.Drawing.Size(75, 23);
            this.acceptBtn.TabIndex = 4;
            this.acceptBtn.Text = "OK";
            this.acceptBtn.UseVisualStyleBackColor = true;
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(354, 103);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 4;
            this.cancelBtn.Text = "Mégse";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // PathVerifyForm
            // 
            this.AcceptButton = this.acceptBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(441, 138);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.acceptBtn);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.targetLBL);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.resourceLBL);
            this.Name = "PathVerifyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "PathVerifyForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label resourceLBL;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label targetLBL;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button acceptBtn;
        private System.Windows.Forms.Button cancelBtn;
    }
}