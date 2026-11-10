using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using sharpcommander.FileSystems;
using sharpcommander.Localization;
using sharpcommander.Operations;

namespace sharpcommander;

public partial class mainForm : Form, ILocalizable
{
    private const string UpdateVersionUrl = "http://jamela.zxq.net/sharp/version.txt";
    private const string UpdatePackageUrl = "http://jamela.zxq.net/sharp/sharpcmd.zip";

    // Clipboard format Explorer uses to tell "cut" from "copy" for a list of files.
    private const string DropEffectFormat = "Preferred DropEffect";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly FileHandlerForm LeftForm; //Left listview
    private readonly FileHandlerForm RightForm; //Right listview

    private readonly string productName; // window caption as designed, without the version
    private readonly ToolStripMenuItem languageMenu = new() { Name = "languageToolStripMenuItem" };

    public mainForm()
    {
        InitializeComponent();
        productName = Text;
        BuildLanguageMenu();
        WireCommands();
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

    /// <summary>Connects the commands that have no handler assigned in the designer.</summary>
    private void WireCommands()
    {
        newToolStripMenuItem.Click += newToolStripButton_Click;
        openToolStripMenuItem.Click += (_, _) => ActivePanel?.ActivateSelected();
        cutToolStripMenuItem.Click += (_, _) => CopyToClipboard(cut: true);
        copyToolStripMenuItem.Click += (_, _) => CopyToClipboard(cut: false);
        pasteToolStripMenuItem.Click += (_, _) => PasteFromClipboard();
        selectAllToolStripMenuItem.Click += (_, _) => SelectAll();

        toolStripButton1.Click += (_, _) => GoToFolder(Environment.SpecialFolder.UserProfile);
        toolStripButton2.Click += (_, _) => GoToFolder(Environment.SpecialFolder.MyDocuments);
        toolStripTextBox1.KeyDown += toolStripTextBox1_KeyDown;

        // Left over from the menu template: a commander has nothing to save or undo,
        // and there is no options dialog or help file.
        saveToolStripMenuItem.Visible = false;
        undoToolStripMenuItem.Visible = false;
        redoToolStripMenuItem.Visible = false;
        toolStripSeparator3.Visible = false;
        toolsToolStripMenuItem.Visible = false;
        contentsToolStripMenuItem.Visible = false;
        indexToolStripMenuItem.Visible = false;
        searchToolStripMenuItem.Visible = false;
    }

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

    #region Panels

    /// <summary>The focused file panel, if any.</summary>
    private FileHandlerForm? ActivePanel => ActiveMdiChild as FileHandlerForm;

    /// <summary>Resolves the focused file panel and the opposite one.</summary>
    private bool TryGetPanels([NotNullWhen(true)] out FileHandlerForm? active, [NotNullWhen(true)] out FileHandlerForm? other)
    {
        active = ActivePanel;
        other = active is null ? null : active == LeftForm ? RightForm : LeftForm;
        return active is not null && other is not null;
    }

    /// <summary>Like <see cref="TryGetPanels"/>, and also fetches the items to work on; tells the user if there are none.</summary>
    private bool TryGetSelection(
        [NotNullWhen(true)] out FileHandlerForm? active,
        [NotNullWhen(true)] out FileHandlerForm? other,
        out IReadOnlyList<FileEntry> items)
    {
        items = [];
        if (!TryGetPanels(out active, out other))
        {
            return false;
        }

        items = active.GetTargets();
        if (items.Count == 0)
        {
            MessageBox.Show(this, Strings.NothingSelected);
            return false;
        }
        return true;
    }

    private void RefreshPanels()
    {
        _ = LeftForm.RefreshAsync();
        _ = RightForm.RefreshAsync();
    }

    /// <summary>Lets the user confirm or change the target folder (a path in the other panel's file system).</summary>
    private string? AskTargetFolder(IReadOnlyList<FileEntry> items, FileHandlerForm targetPanel)
    {
        using var dialog = new PathVerifyForm([.. items.Select(item => item.FullPath)], targetPanel.CurrentPath);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        string folder = dialog.Targettrails.Trim();
        if (folder.Length == 0)
        {
            return null;
        }
        if (!targetPanel.FileSystem.IsLocal)
        {
            return folder.StartsWith('/') ? folder : targetPanel.FileSystem.Combine(targetPanel.CurrentPath, folder);
        }

        try
        {
            // A relative path means "below the folder shown in the target panel".
            return Path.GetFullPath(folder, targetPanel.CurrentPath);
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            MessageBox.Show(this, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Runs a file operation behind the progress dialog, then refreshes both panels.
    /// </summary>
    /// <returns>True if it ran to the end.</returns>
    private bool RunOperation(string status, string? finishedMessage, Action<OperationContext> work)
    {
        toolStripStatusLabel2.Text = status;
        bool completed = ProcessForm.Run(this, status, work);
        toolStripStatusLabel2.Text = "...";

        RefreshPanels();
        if (completed && finishedMessage is not null)
        {
            MessageBox.Show(this, finishedMessage);
        }
        return completed;
    }

    /// <summary>Runs a quick change (rename, new folder) off the UI thread and reports failures.</summary>
    private async Task RunQuickAsync(Action change)
    {
        try
        {
            await Task.Run(change);
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            MessageBox.Show(this, FileSystemErrors.Describe(ex));
        }
        RefreshPanels();
    }

    #endregion

    #region Commands

    private void StartTransfer(bool move)
    {
        if (!TryGetSelection(out var active, out var other, out var items)
            || AskTargetFolder(items, other) is not { } folder)
        {
            return;
        }

        IFileSystem source = active.FileSystem, destination = other.FileSystem;
        RunOperation(
            move ? Strings.StatusMoving : Strings.StatusCopying,
            move ? Strings.MoveFinished : Strings.CopyFinished,
            context => FileOperations.Transfer(context, source, items, destination, folder, move));
    }

    private void Compress()
    {
        if (!TryGetSelection(out var active, out var other, out var items)
            || AskTargetFolder(items, other) is not { } folder)
        {
            return;
        }

        // One item gives the archive its name; several are named after the folder they are in.
        string name = items is [var single]
            ? (single.IsDirectory ? single.Name : Path.GetFileNameWithoutExtension(single.Name))
            : Path.GetFileName(active.CurrentPath.TrimEnd('\\', '/'));
        if (name.Length == 0)
        {
            name = "archive";
        }

        IFileSystem source = active.FileSystem, destination = other.FileSystem;
        string archive = destination.Combine(folder, name + ".zip");
        RunOperation(Strings.StatusCompressing, Strings.CompressFinished,
            context => FileOperations.Compress(context, source, items, destination, archive));
    }

    private void Extract()
    {
        if (!TryGetSelection(out var active, out var other, out var items))
        {
            return;
        }
        if (items.FirstOrDefault(item => !FileOperations.IsArchive(item)) is { } unsupported)
        {
            MessageBox.Show(this, Strings.UnsupportedArchive(unsupported.Name));
            return;
        }
        if (AskTargetFolder(items, other) is not { } folder)
        {
            return;
        }

        IFileSystem source = active.FileSystem, destination = other.FileSystem;
        RunOperation(Strings.StatusExtracting, Strings.ExtractFinished,
            context => FileOperations.Extract(context, source, items, destination, folder));
    }

    private void Delete()
    {
        if (!TryGetSelection(out var active, out _, out var items)
            || !AreYouSureForm.Ask(this, Strings.ConfirmDelete))
        {
            return;
        }

        IFileSystem fileSystem = active.FileSystem;
        RunOperation(Strings.StatusDeleting, Strings.DeleteFinished,
            context => FileOperations.Delete(context, fileSystem, items));
    }

    private async void CreateFolder()
    {
        if (ActivePanel is not { } active)
        {
            return;
        }

        using var dialog = new NewFolderDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.BackFolderName.Length == 0)
        {
            return;
        }

        IFileSystem fileSystem = active.FileSystem;
        string path = fileSystem.Combine(active.CurrentPath, dialog.BackFolderName);
        await RunQuickAsync(() => fileSystem.CreateDirectory(path));
    }

    private async void Rename()
    {
        if (!TryGetSelection(out var active, out _, out var items))
        {
            return;
        }
        if (items is not [var item])
        {
            MessageBox.Show(this, Strings.RenameSingleItem);
            return;
        }

        using var dialog = new NewFolderDialog(Strings.RenameTitle, Strings.RenamePrompt, item.Name);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.BackFolderName is not { Length: > 0 } newName || newName == item.Name)
        {
            return;
        }

        IFileSystem fileSystem = active.FileSystem;
        string target = fileSystem.Combine(fileSystem.GetParent(item.FullPath) ?? active.CurrentPath, newName);
        await RunQuickAsync(() =>
        {
            // A change of letter case only is the same file on Windows: let the rename through.
            bool sameFile = string.Equals(item.FullPath, target, fileSystem.PathComparison);
            if (!sameFile && (fileSystem.FileExists(target) || fileSystem.DirectoryExists(target)))
            {
                throw new IOException(Strings.AlreadyExists(newName));
            }
            fileSystem.Move(item.FullPath, target);
        });
    }

    /// <summary>Opens the chosen file in the text editor, or an empty editor when no file is chosen.</summary>
    private void Edit()
    {
        if (ActivePanel is not { } active)
        {
            return;
        }

        if (active.GetTargets().FirstOrDefault(item => !item.IsDirectory) is { } file)
        {
            active.OpenFile(file, inEditor: true);
        }
        else
        {
            new BuiltInTextEditor().Show();
        }
    }

    /// <summary>Searches the folder of the active panel, and everything below it, for names matching the search box.</summary>
    private void Search()
    {
        if (ActivePanel is not { } active)
        {
            return;
        }

        string pattern = toolStripTextBox1.Text.Trim();
        if (pattern.Length == 0 || pattern == Strings.SearchPrompt)
        {
            MessageBox.Show(this, Strings.SearchPatternRequired);
            toolStripTextBox1.Focus();
            return;
        }

        IFileSystem fileSystem = active.FileSystem;
        string root = active.CurrentPath;
        List<FileEntry> results = [];
        toolStripStatusLabel2.Text = Strings.StatusSearching;
        bool completed = ProcessForm.Run(this, Strings.StatusSearching,
            context => results = FileOperations.Search(context, fileSystem, root, pattern));
        toolStripStatusLabel2.Text = "...";

        if (!completed)
        {
            return;
        }
        if (results.Count == 0)
        {
            MessageBox.Show(this, Strings.NoSearchResults(pattern));
            return;
        }
        active.ShowSearchResults(results, pattern);
    }

    private void GoToFolder(Environment.SpecialFolder folder)
    {
        _ = ActivePanel?.GoToLocalAsync(Environment.GetFolderPath(folder));
    }

    #endregion

    #region Clipboard

    private void CopyToClipboard(bool cut)
    {
        if (toolStripTextBox1.Focused)
        {
            if (cut)
            {
                toolStripTextBox1.Cut();
            }
            else
            {
                toolStripTextBox1.Copy();
            }
            return;
        }

        if (!TryGetSelection(out var active, out _, out var items))
        {
            return;
        }
        if (!active.FileSystem.IsLocal)
        {
            MessageBox.Show(this, Strings.ClipboardLocalOnly);
            return;
        }

        // The same format Explorer uses, so files can be pasted there and the other way round.
        var files = new StringCollection();
        files.AddRange([.. items.Select(item => item.FullPath)]);
        var data = new DataObject();
        data.SetFileDropList(files);
        DragDropEffects effect = cut ? DragDropEffects.Move : DragDropEffects.Copy;
        data.SetData(DropEffectFormat, new MemoryStream(BitConverter.GetBytes((int)effect)));
        Clipboard.SetDataObject(data, copy: true);
    }

    private void PasteFromClipboard()
    {
        if (toolStripTextBox1.Focused)
        {
            toolStripTextBox1.Paste();
            return;
        }
        if (ActivePanel is not { } active || !Clipboard.ContainsFileDropList())
        {
            return;
        }

        List<FileEntry> items = [.. Clipboard.GetFileDropList().Cast<string>()
            .Select(LocalFileSystem.GetEntry)
            .OfType<FileEntry>()];
        if (items.Count == 0)
        {
            return;
        }

        bool move = Clipboard.GetDataObject()?.GetData(DropEffectFormat) is MemoryStream effect
            && effect.Length >= sizeof(int)
            && ((DragDropEffects)BitConverter.ToInt32(effect.ToArray()) & DragDropEffects.Move) != 0;

        IFileSystem destination = active.FileSystem;
        string folder = active.CurrentPath;
        bool completed = RunOperation(
            move ? Strings.StatusMoving : Strings.StatusCopying,
            finishedMessage: null,
            context => FileOperations.Transfer(context, LocalFileSystem.Instance, items, destination, folder, move));
        if (completed && move)
        {
            Clipboard.Clear(); // the files are no longer where the clipboard says
        }
    }

    private void SelectAll()
    {
        if (toolStripTextBox1.Focused)
        {
            toolStripTextBox1.SelectAll();
        }
        else
        {
            ActivePanel?.CheckAll();
        }
    }

    #endregion

    #region FTP

    /// <summary>Asks for the connection details and shows the server in the active panel.</summary>
    private async void ConnectFtp(bool startBlank)
    {
        if (ActivePanel is not { } active)
        {
            return;
        }

        using var dialog = new FtpConnectDialog(startBlank);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var server = new FtpFileSystem(dialog.Connection, dialog.Password, ConfirmUntrustedCertificate);
        bool connected = false;
        toolStripStatusLabel2.Text = Strings.StatusConnecting(dialog.Connection.Host);
        mainToolStrip.Enabled = topToolStrip.Enabled = mainMenu.Enabled = false; // one thing at a time
        UseWaitCursor = true;
        try
        {
            await Task.Run(server.Connect);
            connected = await active.ConnectAsync(server, server.HomePath);
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            MessageBox.Show(this, FileSystemErrors.Describe(ex), dialog.Connection.Host, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            UseWaitCursor = false;
            mainToolStrip.Enabled = topToolStrip.Enabled = mainMenu.Enabled = true;
            toolStripStatusLabel2.Text = "...";
        }

        if (!connected)
        {
            server.Dispose();
        }
        else if (dialog.SaveConnection)
        {
            AppSettings.Current.RememberFtpConnection(dialog.Connection);
        }
    }

    /// <summary>Called (possibly from a background thread) when a server's TLS certificate cannot be verified.</summary>
    private bool ConfirmUntrustedCertificate(string details) => Invoke(() =>
        MessageBox.Show(this, Strings.FtpUntrustedCertificate(details), productName,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes);

    #endregion

    #region Update

    private async Task downloadUpdate()
    {
        if (saveDownloaded.ShowDialog() == DialogResult.OK)
        {
            byte[] package = await Http.GetByteArrayAsync(UpdatePackageUrl);
            await File.WriteAllBytesAsync(saveDownloaded.FileName, package);
            MessageBox.Show(this, Strings.DownloadFinished);
            Process.Start(new ProcessStartInfo(saveDownloaded.FileName) { UseShellExecute = true });
        }
    } //this method downloads the update file

    private async void updateToolStripMenuItem_Click(object? sender, EventArgs e) //update button
    {
        try
        {
            string content = await Http.GetStringAsync(UpdateVersionUrl);
            string? line = new StringReader(content).ReadLine();

            if (!Version.TryParse(line?.Trim(), out Version? latest) ||
                !Version.TryParse(ProductVersion, out Version? current))
            {
                MessageBox.Show(this, Strings.UpdateCheckFailed);
            }
            else if (latest > current)
            {
                await downloadUpdate();
            }
            else
            {
                MessageBox.Show(this, Strings.UpToDate);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or Win32Exception)
        {
            MessageBox.Show(this, ex.Message);
        }
    }

    #endregion

    #region ClickEventHandling

    private void newToolStripButton_Click(object? sender, EventArgs e) => CreateFolder(); //new folder button

    private void saveToolStripButton_Click(object? sender, EventArgs e) => StartTransfer(move: false); //copy button

    private void cutToolStripButton_Click(object? sender, EventArgs e) => StartTransfer(move: true); //move button

    private void toolStripDeleteButton_Click(object? sender, EventArgs e) => Delete(); //delete button

    private void toolStripRenameButton_Click(object? sender, EventArgs e) => Rename(); //rename button

    private void toolStripEditButton_Click(object? sender, EventArgs e) => Edit(); //edit button

    private void searhToolStripButton_Click(object? sender, EventArgs e) => Search();

    private void toolStripTextBox1_Click(object? sender, EventArgs e)
    {
        // Make room for typing, but do not wipe a search term the user wants to correct.
        if (toolStripTextBox1.Text == Strings.SearchPrompt)
        {
            toolStripTextBox1.Text = "";
        }
    }

    private void toolStripTextBox1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            Search();
        }
    }

    private void aboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var newform = new ProgramInfoBox();
        newform.ShowDialog(this);
    } //about the application

    private void exitToolStripMenuItem_Click(object? sender, EventArgs e) => Application.Exit(); //exit button

    private void refreshToolButton_Click(object? sender, EventArgs e) => RefreshPanels();

    private void compressToolButton_Click(object? sender, EventArgs e) => Compress();

    private void decompressToolButton_Click(object? sender, EventArgs e) => Extract();

    /// <summary>Connects the active panel to the most recently used server, or disconnects it if it shows one.</summary>
    private void ftpToolButton_Click(object? sender, EventArgs e)
    {
        if (ActivePanel is { IsRemote: true } connected)
        {
            _ = connected.DisconnectAsync();
        }
        else
        {
            ConnectFtp(startBlank: false);
        }
    }

    private void newftpToolButton_Click(object? sender, EventArgs e) => ConnectFtp(startBlank: true);

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

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        // Files opened from a server were downloaded here.
        try
        {
            if (Directory.Exists(TempFile.Folder))
            {
                Directory.Delete(TempFile.Folder, recursive: true);
            }
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            // Something is still open in another program; Windows cleans the temp folder eventually.
        }
    }

    #endregion
}
