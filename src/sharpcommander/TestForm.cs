using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace sharpcommander
{
    public partial class TestForm : Form
    {
        
        public TestForm()
        {
            InitializeComponent();
            string path = "C:/";

            foreach (string elem in Directory.GetFileSystemEntries(path))
            {
                string name = Path.GetFileNameWithoutExtension(elem);
                string kiterjesztés = Path.GetExtension(elem);

                if (kiterjesztés=="")
                {
                    listView1.Items.Add(name).SubItems.Add("DIR");
                }
                else listView1.Items.Add(name).SubItems.Add(kiterjesztés);
                
                
                
                
                


                
            }

        }


    }
}
