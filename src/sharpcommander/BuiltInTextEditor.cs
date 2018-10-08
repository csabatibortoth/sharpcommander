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
    public partial class BuiltInTextEditor : Form
    {
        public BuiltInTextEditor(string[] text)
        {
            InitializeComponent();
            richTextBox1.Lines = text;
            this.Text = "Sharp Editor 0.1 (Csak Megtekintés)";
        }
    }
}
