using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using sharpcommander.Localization;

namespace sharpcommander;

public partial class mainForm : Form, ILocalizable
{
    private const string UpdateVersionUrl = "http://jamela.zxq.net/sharp/version.txt";
    private const string UpdatePackageUrl = "http://jamela.zxq.net/sharp/sharpcmd.zip";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly FileHandlerForm LeftForm; //Left listview
    private readonly FileHandlerForm RightForm; //Right listview

    // Arguments of the running background operation.
    private string[] source = [];
    private string destination = "";
    private string sourcef = "";
    private string search = "";

    private readonly string productName; // window caption as designed, without the version
    private readonly ToolStripMenuItem languageMenu = new() { Name = "languageToolStripMenuItem" };

    public mainForm()
    {
        InitializeComponent();
        productName = Text;
        BuildLanguageMenu();
        Localizer.Localize(this);

        /*Here is the initialization of the listviews*/
        LeftForm = new FileHandlerForm();
        RightForm = new FileHandlerForm();

        LeftForm.MdiParent = this;
        RightForm.MdiParent = this;

        LeftForm.Width = (Width - 20) / 2;
        RightForm.Width = (Width - 20) / 2;
        LeftForm.Dock = DockStyle.Left;
        RightForm.Dock = DockStyle.Right;
        LeftForm.Show();
        RightForm.Show();
    } //constructor of the main form

    #region Localization

    /// <summary>Adds the language menu: "system default" plus one entry per available translation.</summary>
    private void BuildLanguageMenu()
    {
        languageMenu.Font = helpToolStripMenuItem.Font;
        languageMenu.Image = Properties.Resources.configure;

        languageMenu.DropDownItems.Add(new ToolStripMenuItem { Name = "systemLanguageToolStripMenuItem" });
        languageMenu.DropDownItems.Add(new ToolStripSeparator());
        foreach (var language in Localizer.SupportedLanguages)
        {
            // Each language is listed under its own name so it can be found from any language.
            string name = language.TextInfo.ToTitleCase(language.NativeName);
            languageMenu.DropDownItems.Add(new ToolStripMenuItem(name) { Tag = language.Name });
        }

        foreach (ToolStripMenuItem item in languageMenu.DropDownItems.OfType<ToolStripMenuItem>())
        {
            // No tag = follow the system language.
            item.Click += (_, _) => Localizer.SetLanguage((string?)item.Tag);
        }

        mainMenu.Items.Insert(mainMenu.Items.IndexOf(helpToolStripMenuItem), languageMenu);
    }

    void ILocalizable.ApplyLocalization()
    {
        Text = $"{productName} {ProductVersion} BETA";
        toolStripTextBox1.Text = Strings.SearchPrompt;
        saveDownloaded.Filter = Strings.ZipFilter;

        foreach (ToolStripMenuItem item in languageMenu.DropDownItems.OfType<ToolStripMenuItem>())
        {
            item.Checked = (string?)item.Tag == Localizer.SelectedLanguage;
        }
    }

    #endregion

    #region FunctionsAndMethods

    /// <summary>Resolves the focused file panel and the opposite one.</summary>
    private bool TryGetPanels([NotNullWhen(true)] out FileHandlerForm? active, [NotNullWhen(true)] out FileHandlerForm? other)
    {
        Form? child = ActiveMdiChild;
        if (child == LeftForm)
        {
            (active, other) = (LeftForm, RightForm);
            return true;
        }
        if (child == RightForm)
        {
            (active, other) = (RightForm, LeftForm);
            return true;
        }

        (active, other) = (null, null);
        return false;
    }

    private void RefreshPanels()
    {
        LeftForm.PopulateList();
        RightForm.PopulateList();
    }

    /// <summary>
    /// Asks for the target folder of the checked items of the active panel and starts
    /// <paramref name="worker"/> (copy / move / compress) on them.
    /// </summary>
    private void StartTransfer(BackgroundWorker worker)
    {
        if (worker.IsBusy || !TryGetPanels(out var active, out var other))
        {
            return;
        }
        if (!active.IsChecked())
        {
            MessageBox.Show(Strings.NothingSelected);
            return;
        }

        using var dialog = new PathVerifyForm(active.BackTrails(), other.Text);
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        source = active.BackTrails();
        destination = dialog.Targettrails;
        sourcef = source[0];
        other.Text = destination;
        UseWaitCursor = true;
        worker.RunWorkerAsync();
    }

    private void ShowBusy(string status)
    {
        toolStripStatusLabel2.Text = status;
        toolStripProgressBar1.MarqueeAnimationSpeed = 30;
        toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
    }

    private void ShowFinished(string message)
    {
        RefreshPanels();
        toolStripStatusLabel2.Text = "...";
        toolStripProgressBar1.MarqueeAnimationSpeed = 0;
        toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
        UseWaitCursor = false;
        MessageBox.Show(message);
    }

    private static bool Confirm(string question)
    {
        using var dialog = new AreYouSureForm(question);
        return dialog.ShowDialog() == DialogResult.OK;
    }

    private void copyElement(string[] source, string destination)
    {
        foreach (string item in source)
        {
            copyBackground.ReportProgress(0);
            if (File.Exists(item))
            {
                string target = Path.Combine(destination, Path.GetFileName(item));
                if (!File.Exists(target) || Confirm(Strings.ConfirmOverwriteItem(item)))
                {
                    File.Copy(item, target, true);
                }
            }
            else if (Directory.Exists(item))
            {
                if (!File.Exists(Path.Combine(destination, Path.GetDirectoryName(item) ?? "")))
                {
                    string newdirname = Path.Combine(destination, Path.GetFileName(item));
                    Directory.CreateDirectory(newdirname);
                    copyElement(Directory.GetFileSystemEntries(item), newdirname);
                }
                else if (Confirm(Strings.ConfirmOverwriteItem(item)))
                {
                    string newdirname = Path.Combine(destination, Path.GetDirectoryName(item) ?? "");
                    copyElement(Directory.GetFileSystemEntries(item), newdirname);
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
                string target = Path.Combine(destination, Path.GetFileName(item));
                if (!File.Exists(target))
                {
                    File.Move(item, target);
                }
                else if (Confirm(Strings.ConfirmOverwriteFiles))
                {
                    File.Move(item, target, overwrite: true);
                }
            }
            else if (Directory.Exists(item))
            {
                if (!File.Exists(Path.Combine(destination, Path.GetDirectoryName(item) ?? "")))
                {
                    string newdirname = Path.Combine(destination, Path.GetFileName(item));
                    Directory.CreateDirectory(newdirname);
                    moveElement(Directory.GetFileSystemEntries(item), newdirname);
                    Directory.Delete(item);
                }
                else if (Confirm(Strings.ConfirmOverwriteFolder))
                {
                    string newdirname = Path.Combine(destination, Path.GetDirectoryName(item) ?? "");
                    Directory.CreateDirectory(newdirname);
                    moveElement(Directory.GetFileSystemEntries(item), newdirname);
                    Directory.Delete(item);
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
                while (Directory.Exists(element))
                {
                    try
                    {
                        string[] files = Directory.GetFiles(Path.GetFullPath(element));
                        string[] dirs = Directory.GetDirectories(Path.GetFullPath(element));
                        if (files.Length != 0)
                        {
                            deleteElement(files);
                        }
                        else if (dirs.Length != 0)
                        {
                            deleteElement(dirs);
                        }
                        else
                        {
                            Directory.Delete(element);
                        }
                    }
                    catch (IOException iex)
                    {
                        MessageBox.Show(iex.Message);
                    }
                }
            }
        }
    } //this method delete files and folders

    private void compressElement(string[] source, string destination, string sourcef)
    {
        string dest = Path.Combine(destination, Path.GetFileName(sourcef));
        using FileStream destSTR = File.Create(dest + ".gz");
        foreach (string item in source)
        {
            byte[] data = File.ReadAllBytes(item);
            using var gzip = new GZipStream(destSTR, CompressionMode.Compress);
            if (File.Exists(item))
            {
                gzip.Write(data, 0, data.Length);
            }
            else if (Directory.Exists(item))
            {
                MessageBox.Show(Strings.CannotCompressFolders);
            }
        }
    }

    private async Task downloadUpdate()
    {
        toolStripProgressBar1.Increment(400);
        if (saveDownloaded.ShowDialog() == DialogResult.OK)
        {
            byte[] package = await Http.GetByteArrayAsync(UpdatePackageUrl);
            await File.WriteAllBytesAsync(saveDownloaded.FileName, package);
            MessageBox.Show(Strings.DownloadFinished);
            Process.Start(new ProcessStartInfo(saveDownloaded.FileName) { UseShellExecute = true });
        }
    } //this method downloads the update file

    #endregion

    #region ClickEventHandling

    #region MainToolStripButtons

    private void newToolStripButton_Click(object? sender, EventArgs e)
    {
        if (!TryGetPanels(out var active, out _))
        {
            return;
        }

        using var uj = new NewFolderDialog();
        if (uj.ShowDialog() == DialogResult.OK)
        {
            Directory.CreateDirectory(Path.Combine(active.BackPath, uj.BackFolderName));
            RefreshPanels();
        }
    } //new folder button

    private void saveToolStripButton_Click(object? sender, EventArgs e)
    {
        StartTransfer(copyBackground);
    } //copy button

    private void cutToolStripButton_Click(object? sender, EventArgs e)
    {
        StartTransfer(moveBackground);
    } //move button

    private void toolStripDeleteButton_Click(object? sender, EventArgs e)
    {
        if (deleteBackground.IsBusy || !TryGetPanels(out var active, out _))
        {
            return;
        }
        if (!Confirm(Strings.ConfirmDelete))
        {
            return;
        }
        if (!active.IsChecked())
        {
            MessageBox.Show(Strings.NothingSelected);
            return;
        }

        source = active.BackTrails();
        UseWaitCursor = true;
        deleteBackground.RunWorkerAsync();
    } //delete button

    private void toolStripRenameButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(Strings.UnderDevelopment);
    } //rename button

    private void toolStripEditButton_Click(object? sender, EventArgs e)
    {
        if (TryGetPanels(out _, out _))
        {
            new BuiltInTextEditor(null).Show();
        }
    } //edit button

    private void searhToolStripButton_Click(object? sender, EventArgs e)
    {
        search = toolStripTextBox1.Text;
        toolStripTextBox1.Text = Strings.SearchPrompt;
        MessageBox.Show(Strings.UnderDevelopment);
    }

    private void toolStripTextBox1_Click(object? sender, EventArgs e)
    {
        toolStripTextBox1.Text = "";
    }

    #endregion

    #region MenuToolStripButtons

    private void aboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var newform = new ProgramInfoBox();
        newform.ShowDialog();
    } //about the application

    private void exitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        Application.Exit();
    } //exit button

    private async void updateToolStripMenuItem_Click(object? sender, EventArgs e) //update button
    {
        try
        {
            toolStripProgressBar1.Increment(400);
            string content = await Http.GetStringAsync(UpdateVersionUrl);
            string? line = new StringReader(content).ReadLine();

            if (!Version.TryParse(line?.Trim(), out Version? latest) ||
                !Version.TryParse(ProductVersion, out Version? current))
            {
                MessageBox.Show(Strings.UpdateCheckFailed);
            }
            else if (latest > current)
            {
                toolStripProgressBar1.Value = 0;
                await downloadUpdate();
            }
            else
            {
                MessageBox.Show(Strings.UpToDate);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or Win32Exception)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            toolStripProgressBar1.Value = 0;
        }
    }

    #endregion

    #region TopMenuButtons

    private void refreshToolButton_Click(object? sender, EventArgs e)
    {
        RefreshPanels();
    }

    private void compressToolButton_Click(object? sender, EventArgs e)
    {
        StartTransfer(compressBackground);
    }

    private void decompressToolButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(Strings.UnderDevelopment);
    }

    private void ftpToolButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(Strings.UnderDevelopment);
    }

    private void newftpToolButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(Strings.UnderDevelopment);
    }

    #endregion

    #endregion

    #region BackGroundWorkerEvents

    private void copyBackground_DoWork(object? sender, DoWorkEventArgs e)
    {
        copyElement(source, destination);
    } //copeis files and folders in background

    private void copyBackground_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        ShowBusy(Strings.StatusCopying);
    } //progressbar handling

    private void copyBackground_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        ShowFinished(Strings.CopyFinished);
    } //copy completed

    private void moveBackground_DoWork(object? sender, DoWorkEventArgs e)
    {
        moveElement(source, destination);
    } //moves files and folders in background

    private void moveBackground_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        ShowBusy(Strings.StatusMoving);
    } //progressbar handling

    private void moveBackground_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        ShowFinished(Strings.MoveFinished);
    } //move completed

    private void deleteBackground_DoWork(object? sender, DoWorkEventArgs e)
    {
        deleteElement(source);
    }

    private void deleteBackground_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        ShowBusy(Strings.StatusDeleting);
    }

    private void deleteBackground_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        ShowFinished(Strings.DeleteFinished);
    }

    private void compressBackground_DoWork(object? sender, DoWorkEventArgs e)
    {
        compressElement(source, destination, sourcef);
    }

    private void compressBackground_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        ShowBusy(Strings.StatusCompressing);
    }

    private void compressBackground_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        ShowFinished(Strings.CompressFinished);
    }

    #endregion

    #region FormHandling //window reszie

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // Also raised from InitializeComponent(), before the panels exist.
        if (LeftForm is null || RightForm is null)
        {
            return;
        }

        LeftForm.Width = (Width - 20) / 2;
        RightForm.Width = (Width - 20) / 2;
        LeftForm.Invalidate();
        RightForm.Invalidate();
    }

    #endregion
}
