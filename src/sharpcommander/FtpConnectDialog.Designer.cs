namespace sharpcommander
{
    partial class FtpConnectDialog
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
            this.savedLbl = new System.Windows.Forms.Label();
            this.savedCombo = new System.Windows.Forms.ComboBox();
            this.hostLbl = new System.Windows.Forms.Label();
            this.hostBox = new System.Windows.Forms.TextBox();
            this.portLbl = new System.Windows.Forms.Label();
            this.portBox = new System.Windows.Forms.NumericUpDown();
            this.userLbl = new System.Windows.Forms.Label();
            this.userBox = new System.Windows.Forms.TextBox();
            this.passwordLbl = new System.Windows.Forms.Label();
            this.passwordBox = new System.Windows.Forms.TextBox();
            this.securityLbl = new System.Windows.Forms.Label();
            this.securityCombo = new System.Windows.Forms.ComboBox();
            this.saveCheck = new System.Windows.Forms.CheckBox();
            this.connectBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.portBox)).BeginInit();
            this.SuspendLayout();
            //
            // savedLbl
            //
            this.savedLbl.AutoSize = true;
            this.savedLbl.Location = new System.Drawing.Point(12, 15);
            this.savedLbl.Name = "savedLbl";
            this.savedLbl.Size = new System.Drawing.Size(98, 13);
            this.savedLbl.TabIndex = 0;
            this.savedLbl.Text = "Mentett kapcsolat:";
            //
            // savedCombo
            //
            this.savedCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.savedCombo.FormattingEnabled = true;
            this.savedCombo.Location = new System.Drawing.Point(130, 12);
            this.savedCombo.Name = "savedCombo";
            this.savedCombo.Size = new System.Drawing.Size(242, 21);
            this.savedCombo.TabIndex = 1;
            this.savedCombo.SelectedIndexChanged += new System.EventHandler(this.savedCombo_SelectedIndexChanged);
            //
            // hostLbl
            //
            this.hostLbl.AutoSize = true;
            this.hostLbl.Location = new System.Drawing.Point(12, 50);
            this.hostLbl.Name = "hostLbl";
            this.hostLbl.Size = new System.Drawing.Size(60, 13);
            this.hostLbl.TabIndex = 2;
            this.hostLbl.Text = "Kiszolgáló:";
            //
            // hostBox
            //
            this.hostBox.Location = new System.Drawing.Point(130, 47);
            this.hostBox.Name = "hostBox";
            this.hostBox.Size = new System.Drawing.Size(242, 20);
            this.hostBox.TabIndex = 3;
            //
            // portLbl
            //
            this.portLbl.AutoSize = true;
            this.portLbl.Location = new System.Drawing.Point(12, 76);
            this.portLbl.Name = "portLbl";
            this.portLbl.Size = new System.Drawing.Size(29, 13);
            this.portLbl.TabIndex = 4;
            this.portLbl.Text = "Port:";
            //
            // portBox
            //
            this.portBox.Location = new System.Drawing.Point(130, 73);
            this.portBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.portBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(70, 20);
            this.portBox.TabIndex = 5;
            this.portBox.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            //
            // userLbl
            //
            this.userLbl.AutoSize = true;
            this.userLbl.Location = new System.Drawing.Point(12, 102);
            this.userLbl.Name = "userLbl";
            this.userLbl.Size = new System.Drawing.Size(87, 13);
            this.userLbl.TabIndex = 6;
            this.userLbl.Text = "Felhasználónév:";
            //
            // userBox
            //
            this.userBox.Location = new System.Drawing.Point(130, 99);
            this.userBox.Name = "userBox";
            this.userBox.Size = new System.Drawing.Size(242, 20);
            this.userBox.TabIndex = 7;
            //
            // passwordLbl
            //
            this.passwordLbl.AutoSize = true;
            this.passwordLbl.Location = new System.Drawing.Point(12, 128);
            this.passwordLbl.Name = "passwordLbl";
            this.passwordLbl.Size = new System.Drawing.Size(39, 13);
            this.passwordLbl.TabIndex = 8;
            this.passwordLbl.Text = "Jelszó:";
            //
            // passwordBox
            //
            this.passwordBox.Location = new System.Drawing.Point(130, 125);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.Size = new System.Drawing.Size(242, 20);
            this.passwordBox.TabIndex = 9;
            this.passwordBox.UseSystemPasswordChar = true;
            //
            // securityLbl
            //
            this.securityLbl.AutoSize = true;
            this.securityLbl.Location = new System.Drawing.Point(12, 154);
            this.securityLbl.Name = "securityLbl";
            this.securityLbl.Size = new System.Drawing.Size(56, 13);
            this.securityLbl.TabIndex = 10;
            this.securityLbl.Text = "Titkosítás:";
            //
            // securityCombo
            //
            this.securityCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.securityCombo.FormattingEnabled = true;
            this.securityCombo.Location = new System.Drawing.Point(130, 151);
            this.securityCombo.Name = "securityCombo";
            this.securityCombo.Size = new System.Drawing.Size(242, 21);
            this.securityCombo.TabIndex = 11;
            //
            // saveCheck
            //
            this.saveCheck.AutoSize = true;
            this.saveCheck.Checked = true;
            this.saveCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.saveCheck.Location = new System.Drawing.Point(130, 182);
            this.saveCheck.Name = "saveCheck";
            this.saveCheck.Size = new System.Drawing.Size(196, 17);
            this.saveCheck.TabIndex = 12;
            this.saveCheck.Text = "Kapcsolat mentése (jelszó nélkül)";
            this.saveCheck.UseVisualStyleBackColor = true;
            //
            // connectBtn
            //
            this.connectBtn.Location = new System.Drawing.Point(206, 214);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(85, 23);
            this.connectBtn.TabIndex = 13;
            this.connectBtn.Text = "Kapcsolódás";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            //
            // cancelBtn
            //
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(297, 214);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 14;
            this.cancelBtn.Text = "Mégse";
            this.cancelBtn.UseVisualStyleBackColor = true;
            //
            // FtpConnectDialog
            //
            this.AcceptButton = this.connectBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(384, 249);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.saveCheck);
            this.Controls.Add(this.securityCombo);
            this.Controls.Add(this.securityLbl);
            this.Controls.Add(this.passwordBox);
            this.Controls.Add(this.passwordLbl);
            this.Controls.Add(this.userBox);
            this.Controls.Add(this.userLbl);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.portLbl);
            this.Controls.Add(this.hostBox);
            this.Controls.Add(this.hostLbl);
            this.Controls.Add(this.savedCombo);
            this.Controls.Add(this.savedLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FtpConnectDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FTP kapcsolat";
            ((System.ComponentModel.ISupportInitialize)(this.portBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label savedLbl;
        private System.Windows.Forms.ComboBox savedCombo;
        private System.Windows.Forms.Label hostLbl;
        private System.Windows.Forms.TextBox hostBox;
        private System.Windows.Forms.Label portLbl;
        private System.Windows.Forms.NumericUpDown portBox;
        private System.Windows.Forms.Label userLbl;
        private System.Windows.Forms.TextBox userBox;
        private System.Windows.Forms.Label passwordLbl;
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.Label securityLbl;
        private System.Windows.Forms.ComboBox securityCombo;
        private System.Windows.Forms.CheckBox saveCheck;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.Button cancelBtn;
    }
}
