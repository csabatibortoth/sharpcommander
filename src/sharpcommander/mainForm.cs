using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;

namespace sharpcommander
{
    public partial class mainForm : Form
    {
        FileHandlerForm LeftForm; //Left listview
        FileHandlerForm RightForm; //Right listview
        string[] source; string destination; string sourcef;
        string search;

        public mainForm()
        {
            InitializeComponent();

            this.Text += " " + ProductVersion + " BETA";

            /*Here is the initialization of the listviews*/
            LeftForm = new FileHandlerForm();
            RightForm = new FileHandlerForm();

            LeftForm.MdiParent = this;
            RightForm.MdiParent = this;

            LeftForm.Width = (this.Width - 20) / 2;
            RightForm.Width = (this.Width - 20) / 2;
            LeftForm.Dock = DockStyle.Left;
            RightForm.Dock = DockStyle.Right;
            LeftForm.Show();
            RightForm.Show();


        } //constructor of the main form

        #region FunctionsAndMethods

        private void copyElement(string[] source, string destination)
        {
            foreach (string item in source)
            {
                string dest = Path.Combine(destination, Path.GetFileName(item));
                copyBackground.ReportProgress(0);
                if (File.Exists(item))
                {
                    if (!File.Exists(Path.Combine(destination, Path.GetFileName(item))))
                    {
                        File.Copy(item, Path.Combine(destination, Path.GetFileName(item)), true);
                    }
                    else
                    {
                        AreYouSureForm newform2 = new AreYouSureForm("Biztos, hogy felülírja a " + item + "?");
                        if (newform2.ShowDialog() == DialogResult.OK)
                        {
                            File.Copy(item, Path.Combine(destination, Path.GetFileName(item)), true);
                        }
                    }
                }
                else
                {
                    if (Directory.Exists(item))
                    {
                        if (!File.Exists(Path.Combine(destination, Path.GetDirectoryName(item))))
                        {
                            string newdirname = Path.Combine(destination, Path.GetFileName(item));
                            Directory.CreateDirectory(newdirname);
                            string[] newitems = Directory.GetFileSystemEntries(item);
                            copyElement(newitems, newdirname);
                        }
                        else
                        {
                            AreYouSureForm newform3 = new AreYouSureForm("Biztos, hogy felülírja a " + item + "?");
                            if (newform3.ShowDialog() == DialogResult.OK)
                            {
                                string newdirname = Path.Combine(destination, Path.GetDirectoryName(item));
                                string[] newitems = Directory.GetFileSystemEntries(item);
                                copyElement(newitems, newdirname);
                            }
                        }
                    }
                }
            }
        } //this method copy the files and the folders

        private void moveElement(string[] source, string destination)
        {
            foreach (string item in source)
            {
                if (File.Exists(item))
                {
                    if (!File.Exists(Path.Combine(destination, Path.GetFileName(item))))
                    {
                        File.Move(item, Path.Combine(destination, Path.GetFileName(item)));
                    }
                    else
                    {
                        AreYouSureForm newform2 = new AreYouSureForm("Biztos, hogy felülírja a Fájlokat?");
                        if (newform2.ShowDialog() == DialogResult.OK)
                        {
                            File.Delete(Path.Combine(destination, Path.GetFileName(item)));
                            File.Move(item, Path.Combine(destination, Path.GetFileName(item)));
                        }
                    }
                }
                else
                {
                    if (Directory.Exists(item))
                    {
                        if (!File.Exists(Path.Combine(destination, Path.GetDirectoryName(item))))
                        {
                            string newdirname = Path.Combine(destination, Path.GetFileName(item));
                            Directory.CreateDirectory(newdirname);
                            string[] newitems = Directory.GetFileSystemEntries(item);
                            moveElement(newitems, newdirname);
                            Directory.Delete(item);
                        }
                        else
                        {
                            AreYouSureForm newform3 = new AreYouSureForm("Biztos, hogy felülírja a Mappa tartalmát?");
                            if (newform3.ShowDialog() == DialogResult.OK)
                            {
                                string newdirname = Path.Combine(destination, Path.GetDirectoryName(item));
                                Directory.CreateDirectory(newdirname);
                                string[] newitems = Directory.GetFileSystemEntries(item);
                                moveElement(newitems, newdirname);
                                Directory.Delete(item);

                            }
                        }
                    }
                }
            }
        } //this method move the files and the folders 

        private void deleteElement(string[] source)
        {
            foreach (string element in source)
            {
                if (File.Exists(element))
                {
                    while (File.Exists(element))
                    {
                        File.Delete(element);
                        deleteElement(source);
                    }
                }
                else
                {
                    if (Directory.Exists(element))
                    {
                        while (Directory.Exists(element))
                        {
                            try
                            {
                                string[] files = Directory.GetFiles(Path.GetFullPath(element));
                                string[] dirs = Directory.GetDirectories(Path.GetFullPath(element));
                                if (files.Length != 0)
                                {
                                    deleteElement(files);
                                    //deleteElement(dirs);
                                }
                                else
                                {
                                    if (dirs.Length != 0)
                                    {
                                        //deleteElement(files);
                                        deleteElement(dirs);
                                    }
                                    else
                                    {
                                        Directory.Delete(element);
                                    }

                                }

                            }
                            catch (IOException iex)
                            {
                                MessageBox.Show(iex.Message);
                            }
                            //Directory.Delete(element); 
                        }
                    }
                }
            }

        } //this method delete files and folders

        private void compressElement(string[] source, string destination, string sourcef)
        {
            string dest = Path.Combine(destination, Path.GetFileName(sourcef));
            using (FileStream destSTR = File.Create(dest+".gz"))
            {
                foreach (string item in source)
                {
                    
		               byte[] data = File.ReadAllBytes(item);
                       using (GZipStream gzip = new GZipStream(destSTR,CompressionMode.Compress))
                       {
                            if (File.Exists(item))
                            {
                                gzip.Write(data, 0, data.Length);
                            }
                            else
                            {
                                if (Directory.Exists(item))
                                {
                                    MessageBox.Show("Mappákat még nem tudok tömöríteni!");
                                }
                            }
                       } 
	                
                    
                }
            }
        }




        private void downloadUpdate()
        {
            System.Net.WebClient newclient = new System.Net.WebClient();
            toolStripProgressBar1.Increment(400);
            if (saveDownloaded.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveDownloaded.FileName, newclient.DownloadData("http://jamela.zxq.net/sharp/sharpcmd.zip"));
                MessageBox.Show("Letöltés befejezve! Manuális telepítés!");
                System.Diagnostics.Process.Start(saveDownloaded.FileName);

            }
        } //this method downloads the update file

        #endregion

        #region ClickEventHandling

        #region MainToolStripButtons

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            NewFolderDialog uj = new NewFolderDialog();
            if (this.ActiveMdiChild == LeftForm)
            {
                if (uj.ShowDialog() == DialogResult.OK)
                {
                    string eleres = Path.Combine(LeftForm.BackPath, uj.BackFolderName);
                    Directory.CreateDirectory(eleres);
                    LeftForm.PopulateList();
                    RightForm.PopulateList();
                }
            }
            else
            {
                if (this.ActiveMdiChild == RightForm)
                {
                    if (uj.ShowDialog() == DialogResult.OK)
                    {
                        string path = Path.Combine(RightForm.BackPath, uj.BackFolderName);
                        Directory.CreateDirectory(path);
                        LeftForm.PopulateList();
                        RightForm.PopulateList();
                    }
                }
            }
        } //new folder button

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == LeftForm)
            {
                PathVerifyForm newform = new PathVerifyForm(LeftForm.BackTrails(), RightForm.Text);
                if (LeftForm.IsChecked())
                {
                    if (newform.ShowDialog() == DialogResult.OK)
                    {
                        source = LeftForm.BackTrails();
                        destination = newform.Targettrails;
                        RightForm.Text = destination;
                        copyBackground.RunWorkerAsync();

                    }
                }
                else MessageBox.Show("Még nem választott ki elemet!");
            }
            else
            {
                if (this.ActiveMdiChild == RightForm)
                {
                    PathVerifyForm newform = new PathVerifyForm(RightForm.BackTrails(), LeftForm.Text);
                    if (RightForm.IsChecked())
                    {
                        if (newform.ShowDialog() == DialogResult.OK)
                        {

                            source = RightForm.BackTrails();
                            destination = newform.Targettrails;
                            LeftForm.Text = destination;
                            copyBackground.RunWorkerAsync();

                        }
                    }
                    else MessageBox.Show("Még nem választott ki elemet!");
                }
            }

        } //copy button

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == LeftForm)
            {
                PathVerifyForm newform = new PathVerifyForm(LeftForm.BackTrails(), RightForm.Text);
                if (LeftForm.IsChecked())
                {
                    if (newform.ShowDialog() == DialogResult.OK)
                    {
                        source = LeftForm.BackTrails();
                        destination = newform.Targettrails;
                        RightForm.Text = destination;
                        moveBackground.RunWorkerAsync();
                    }
                }
                else MessageBox.Show("Még nem választott ki elemet!");
            }
            else
            {
                if (this.ActiveMdiChild == RightForm)
                {
                    PathVerifyForm newform = new PathVerifyForm(RightForm.BackTrails(), LeftForm.Text);
                    if (RightForm.IsChecked())
                    {
                        if (newform.ShowDialog() == DialogResult.OK)
                        {
                            source = RightForm.BackTrails();
                            destination = newform.Targettrails;
                            LeftForm.Text = destination;
                            moveBackground.RunWorkerAsync();
                        }
                    }
                    else MessageBox.Show("Még nem választott ki elemet!");
                }
            }
        } //move button

        private void toolStripDeleteButton_Click(object sender, EventArgs e)
        {
            AreYouSureForm newform = new AreYouSureForm("Biztos, hogy törli kijelölt elemeket?");
            if (this.ActiveMdiChild == LeftForm)
            {
                if (newform.ShowDialog() == DialogResult.OK)
                {
                    if (LeftForm.IsChecked())
                    {
                        source = LeftForm.BackTrails();
                        deleteBackground.RunWorkerAsync();
                    }
                    else MessageBox.Show("Még nem választott ki elemet!");
                }
                //LeftForm.PopulateList();
                //RightForm.PopulateList();
            }
            else
            {
                if (this.ActiveMdiChild == RightForm)
                {
                    if (newform.ShowDialog() == DialogResult.OK)
                    {
                        if (RightForm.IsChecked())
                        {
                            source = RightForm.BackTrails();
                            deleteBackground.RunWorkerAsync();

                            //foreach (string element in RightForm.BackTrails())
                            //{
                            //    if (Path.HasExtension(element))
                            //    {
                            //        File.Delete(element);
                            //    }
                            //    else
                            //    {
                            //        try
                            //        {
                            //            Directory.Delete(element);
                            //        }
                            //        catch (IOException iex)
                            //        {
                            //            MessageBox.Show(iex.Message);
                            //        }
                            //    }

                            //}
                        }
                        else MessageBox.Show("Még nem választott ki elemet!");
                    }
                    //LeftForm.PopulateList();
                    //RightForm.PopulateList();
                }
            }
        } //delete button

        private void toolStripRenameButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fejlesztés alatt");
        } //rename button

        private void toolStripEditButton_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == LeftForm)
            {
                BuiltInTextEditor newform = new BuiltInTextEditor(null);
                newform.Show();
            }
            else
            {
                if (this.ActiveMdiChild == RightForm)
                {
                    BuiltInTextEditor newform = new BuiltInTextEditor(null);
                    newform.Show();
                }
            }
        } //edit button

        private void searhToolStripButton_Click(object sender, EventArgs e)
        {
            search = toolStripTextBox1.Text;
            toolStripTextBox1.Text = "Mit akarsz keresni?";
            MessageBox.Show("Fejlesztés alatt");
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            toolStripTextBox1.Text = "";
        }

        #endregion

        #region MenuToolStripButtons
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProgramInfoBox newform = new ProgramInfoBox();
            if (newform.ShowDialog() == DialogResult.OK)
            {

            }
        } //about the application

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        } //exit button

        private void updateToolStripMenuItem_Click(object sender, EventArgs e) //update button
        {

            System.Net.WebClient newclient = new System.Net.WebClient();
            Stream strm = newclient.OpenRead("http://jamela.zxq.net/sharp/version.txt");
            StreamReader sr = new StreamReader(strm);
            string line = sr.ReadLine();
            string[] oldver = ProductVersion.ToString().Split('.');
            string[] newver = line.Split('.');
            int[] oldversion = new int[4];
            int[] newversion = new int[4];
            bool isnew = true;
            for (int i = 0; i < 4; i++)
            {
                toolStripProgressBar1.Increment(400);
                if (int.Parse(newver[i]) > int.Parse(oldver[i]))
                {
                    toolStripProgressBar1.Value = 0;
                    downloadUpdate();
                    isnew = false;
                    break;
                }
            }
            if (isnew)
            {
                MessageBox.Show("Legújabb verziót használja");
                toolStripProgressBar1.Value = 0;
            }
            strm.Close();
        }
        #endregion

        #region TopMenuButtons

        private void refreshToolButton_Click(object sender, EventArgs e)
        {
            LeftForm.PopulateList();
            RightForm.PopulateList();
        }

        private void compressToolButton_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == LeftForm)
            {
                PathVerifyForm newform = new PathVerifyForm(LeftForm.BackTrails(), RightForm.Text);
                if (LeftForm.IsChecked())
                {
                    if (newform.ShowDialog() == DialogResult.OK)
                    {
                        source = LeftForm.BackTrails();
                        destination = newform.Targettrails;
                        RightForm.Text = destination;
                        sourcef = LeftForm.BackTrails()[0];
                        compressBackground.RunWorkerAsync();

                    }
                }
                else MessageBox.Show("Még nem választott ki elemet!");
            }
            else
            {
                if (this.ActiveMdiChild == RightForm)
                {
                    PathVerifyForm newform = new PathVerifyForm(RightForm.BackTrails(), LeftForm.Text);
                    if (RightForm.IsChecked())
                    {
                        if (newform.ShowDialog() == DialogResult.OK)
                        {

                            source = RightForm.BackTrails();
                            destination = newform.Targettrails;
                            LeftForm.Text = destination;
                            compressBackground.RunWorkerAsync();

                        }
                    }
                    else MessageBox.Show("Még nem választott ki elemet!");
                }
            }
        }

        private void decompressToolButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fejlesztés alatt...");
        }

        private void ftpToolButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fejlesztés alatt...");
        }

        private void newftpToolButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fejlesztés alatt...");
        }

        #endregion

        #endregion

        #region BackGroundWorkerEvents

        private void copyBackground_DoWork(object sender, DoWorkEventArgs e)
        {
            this.UseWaitCursor = true;
            copyElement(source, destination);
        } //copeis files and folders in background

        private void copyBackground_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel2.Text = "Másolás...";
            toolStripProgressBar1.MarqueeAnimationSpeed = 30;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
        } //progressbar handling

        private void copyBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LeftForm.PopulateList();
            RightForm.PopulateList();
            toolStripStatusLabel2.Text = "...";
            toolStripProgressBar1.MarqueeAnimationSpeed = 0;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
            this.UseWaitCursor = false;
            MessageBox.Show("Másolás befejezve!");
        } //copy completed

        private void moveBackground_DoWork(object sender, DoWorkEventArgs e)
        {
            this.UseWaitCursor = true;
            moveElement(source, destination);
        } //moves files and folders in background

        private void moveBackground_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel2.Text = "Áthelyezés...";
            toolStripProgressBar1.MarqueeAnimationSpeed = 30;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
        } //progressbar handling

        private void moveBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LeftForm.PopulateList();
            RightForm.PopulateList();
            toolStripStatusLabel2.Text = "...";
            toolStripProgressBar1.MarqueeAnimationSpeed = 0;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
            this.UseWaitCursor = false;
            MessageBox.Show("Áthelyezés befejezve!");
        } //move completed

        private void deleteBackground_DoWork(object sender, DoWorkEventArgs e)
        {
            this.UseWaitCursor = true;
            deleteElement(source);
        }

        private void deleteBackground_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel2.Text = "Törlés...";
            toolStripProgressBar1.MarqueeAnimationSpeed = 30;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void deleteBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LeftForm.PopulateList();
            RightForm.PopulateList();
            toolStripStatusLabel2.Text = "...";
            toolStripProgressBar1.MarqueeAnimationSpeed = 0;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
            this.UseWaitCursor = false;
            MessageBox.Show("Törlés befejezve!");
        }

        private void compressBackground_DoWork(object sender, DoWorkEventArgs e)
        {
            this.UseWaitCursor = true;
            compressElement(source, destination, sourcef);
        }

        private void compressBackground_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel2.Text = "Tömörítés...";
            toolStripProgressBar1.MarqueeAnimationSpeed = 30;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void compressBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LeftForm.PopulateList();
            RightForm.PopulateList();
            toolStripStatusLabel2.Text = "...";
            toolStripProgressBar1.MarqueeAnimationSpeed = 0;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
            this.UseWaitCursor = false;
            MessageBox.Show("Tömörítés befejezve!");
        }

        #endregion

        #region FormHandling //window reszie

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            try
            {
                LeftForm.Width = (this.Width - 20) / 2;
                RightForm.Width = (this.Width - 20) / 2;



                LeftForm.Invalidate();
                RightForm.Invalidate();
            }
            catch (Exception)
            {

            }
        }

        #endregion














    }
}

