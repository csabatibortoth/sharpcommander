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
    public partial class PathVerifyForm : Form
    {
        string[] sourcetrails;
        string targettrails;

        public string Targettrails
        {
            get { return targettrails; }
            set { targettrails = value; }
        }

        public PathVerifyForm(string[] sourcetrails, string targettrail)
        {
            InitializeComponent();
            this.sourcetrails = sourcetrails;
            this.targettrails = targettrail;
            textBox2.Text = this.targettrails;
            if (this.sourcetrails.Length == 1)
            {
                textBox1.Text = this.sourcetrails[0];
            }
            else textBox1.Text = @"\*";
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Targettrails = textBox2.Text;
        }



    }
}
