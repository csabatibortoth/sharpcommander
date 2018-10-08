using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sharpcommander
{
    public partial class NewFolderDialog : Form
    {
        public string BackFolderName { get { return textBox1.Text; } }
        
        public NewFolderDialog()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar==Convert.ToChar(Keys.Enter))
            {
                this.DialogResult = DialogResult.OK;
            }
        } //exit to enter button
    }
}
