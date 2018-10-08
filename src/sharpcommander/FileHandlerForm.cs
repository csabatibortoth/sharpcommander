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


            int index = 0;
            foreach (object element in Directory.GetLogicalDrives())
            {
                comboBox1.Items.Add(element);
                if (element.ToString()=="C:\\")
                {
                    index = comboBox1.Items.IndexOf(element);
                }
            }
            comboBox1.SelectedIndex = index;
        } //constructor of File Handler form

        #region FormHandling

        public void PopulateList()
        {
                listView1.Items.Clear();
                listView1.Items.Add("/.../");
                try
                {
                    try
                    {
                        foreach (string element in Directory.GetDirectories(this.Text))
                        {
                            string name = Path.GetFileNameWithoutExtension(element.ToString());
                            string[] misc = new string[4];
                            FileInfo info = new FileInfo(element.ToString());
                            misc[0] = "DIR";
                            try
                            {
                                misc[1] = info.Length.ToString();
                            }
                            catch (FileNotFoundException)
                            {
                                misc[1] = " ";
                            }
                            misc[2] = Directory.GetCreationTimeUtc(element.ToString()).ToString();
                            misc[3] = element.ToString();
                            listView1.Items.Add(name).SubItems.AddRange(misc);
                        }
                        foreach (string element in Directory.GetFiles(this.Text))
                        {
                            string name = Path.GetFileNameWithoutExtension(element.ToString());
                            string[] misc = new string[4];
                            FileInfo info = new FileInfo(element.ToString());
                            misc[0] = Path.GetExtension(element.ToString());
                            try
                            {
                                misc[1] = info.Length.ToString();
                            }
                            catch (FileNotFoundException)
                            {
                                misc[1] = " ";
                            }
                            misc[2] = Directory.GetCreationTimeUtc(element.ToString()).ToString();
                            misc[3] = element.ToString();
                            listView1.Items.Add(name).SubItems.AddRange(misc);
                        }
                        

                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Nincs megfelelő joga a mappa megnyitásához!");
                        this.Text = comboBox1.SelectedItem.ToString();
                        PopulateList();
                    }

                }
                catch (IOException iex)
                {
                    MessageBox.Show(iex.Message);
                    comboBox1.SelectedIndex = 0;
                }

                
            } //this method used to populate listview

        private void RunApp(string spath)
        {
            string path = spath;
            //this.Text = Path.GetDirectoryName(this.Text);
            switch (Path.GetExtension(path))
            {
                #region TextFiles
                case ".txt": OpenTxt(path);
                    break; 
                #endregion

                #region Pictures

                #endregion

                #region Videos

                #endregion

                default: System.Diagnostics.Process.Start(path);
                break;
            }
        } //run application for proper extensions

        private void OpenTxt(string source)
        {
            string[] text = File.ReadAllLines(source, Encoding.Default);
            BuiltInTextEditor neweditor = new BuiltInTextEditor(text);
            neweditor.Show();
        } //this method for opening .txt files

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Text = comboBox1.SelectedItem.ToString();
            PopulateList();

        }  //this is for the drive letter change

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            try
            {
                if (listView1.Items.IndexOf(listView1.SelectedItems[0]) != 0)
                {
                    if (listView1.SelectedItems[0].SubItems[0].Text != "/.../")
                    {
                        if (listView1.SelectedItems[0].SubItems[1].Text == "DIR")
                        {
                            try
                            {
                                this.Text = listView1.SelectedItems[0].SubItems[4].Text;
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                PopulateList();
                            }
                            PopulateList();
                        }
                        else
                        {
                            RunApp(listView1.SelectedItems[0].SubItems[4].Text);
                        }
                    }
                    else
                    {
                        string parent = FileAcces.backparent(this.Text);
                        this.Text = parent;
                        PopulateList();
                    }
                }
                else
                {
                    string parent = FileAcces.backparent(this.Text);
                    this.Text = parent;
                    PopulateList();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                PopulateList();
            }
            
        } //this is for entering to a folder 

        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar==Convert.ToChar(Keys.Enter))
            {
                if (listView1.Items.IndexOf(listView1.SelectedItems[0]) != 0)
                {
                    try
                    {
                        this.Text = listView1.SelectedItems[0].SubItems[4].Text;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        PopulateList();
                    }
                    PopulateList();
                }
                else
                {
                    string parent = FileAcces.backparent(this.Text);
                    this.Text = parent;
                    PopulateList();
                } 
            }


        }  //this is for using enter button

        private void FileHandlerForm_Enter(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            pictureBox2.Visible = false;
        }  //change picture state

        private void FileHandlerForm_Leave(object sender, EventArgs e)
        {
            pictureBox1.Visible = false;
            pictureBox2.Visible = true;
        } //change picture state

        #endregion

        #region CommunicateOtherObjects

        public bool IsChecked()
        {
            //MessageBox.Show(listView1.CheckedItems.Count.ToString());
            if (listView1.CheckedItems.Count != 0)
            {
                return true;
            }
            else return false;
        } //returns true when listview item is checked

        public string BackPath { get { return this.Text; } } //this gets back the path of the current folder

        public string[] BackTrails()
        {
            string[] trails = new string[listView1.CheckedItems.Count];
            for (int i = 0; i < listView1.CheckedItems.Count; i++)
            {
                trails[i] = listView1.CheckedItems[i].SubItems[4].Text;
            }
            return trails;
        } //gets back the selected items trails

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        } //this gets back the trails of the chechked items 

        #endregion



    }
}
