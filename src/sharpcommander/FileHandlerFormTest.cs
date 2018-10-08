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
    public partial class FileHandlerForm : Form
    {
        public FileHandlerForm()
        {
            InitializeComponent();

            foreach (object element in Directory.GetLogicalDrives())
            {
                comboBox1.Items.Add(element);
            }
            comboBox1.SelectedIndex = 0;
        }

        public void Refreshlist()
        {
            listBox1.Items.Clear();
            listBox1.Items.Add("/.../");
            foreach (object element in Directory.GetFileSystemEntries(this.Text))
            {
                listBox1.Items.Add(element);
            }
        }

        public string BackPath { get { return this.Text; } }
        public string[] BackTrails()
        {
            string[] trails = new string[listBox1.SelectedItems.Count];
            for (int i = 0; i < listBox1.SelectedItems.Count; i++)
            {
                trails[i] = listBox1.SelectedItems[i].ToString();
            }
            return trails;
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Text = comboBox1.SelectedItem.ToString();
            listBox1.Items.Clear();
            listBox1.Items.Add("/.../");
            try
            {
                foreach (object element in Directory.GetFileSystemEntries(comboBox1.SelectedItem.ToString()))
                {
                    listBox1.Items.Add(element);
                }
            }
            catch (IOException iex)
            {
                MessageBox.Show(iex.Message);
                comboBox1.SelectedIndex = 0;
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex != 0)
            {
                this.Text = listBox1.SelectedItem.ToString();
                FileAcces uj = new FileAcces(listBox1.SelectedItem.ToString());
                listBox1.Items.Clear();
                listBox1.Items.Add("/.../");
                try
                {
                    foreach (object element in uj.backpath())
                    {
                        listBox1.Items.Add(element);
                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Még nem tudok megnyitni Fájlokat!");
                    comboBox1.SelectedIndex = 0;
                }
            }
            else
            {
                string parent = FileAcces.backparent(this.Text);
                this.Text = parent;
                listBox1.Items.Clear();
                listBox1.Items.Add("/.../");
                foreach (object element in Directory.GetFileSystemEntries(parent))
                {
                    listBox1.Items.Add(element);
                }

            }

        }
    }
}
