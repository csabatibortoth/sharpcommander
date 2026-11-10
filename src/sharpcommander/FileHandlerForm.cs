using System.ComponentModel;
using System.Diagnostics;
using sharpcommander.FileSystems;
using sharpcommander.Localization;
using sharpcommander.Operations;

namespace sharpcommander;

/// <summary>One file panel: browses a folder of the local disks or of an FTP server.</summary>
public partial class FileHandlerForm : Form, ILocalizable
{
    private const string ParentEntry = "/.../";
    private const int FolderImage = 0;
    private const int FileImage = 1;

    private IFileSystem fileSystem = LocalFileSystem.Instance;
    private string currentPath = "";

    // Listings are loaded in the background; only the answer to the latest request is shown.
    private int loadVersion;
    private bool showingSearchResults;
    private bool selectingDrive;
    private int currentDriveIndex = -1;

    // The row under the first click of a double click, and whether it was checked then.
    private ListViewItem? clickedRow;
    private bool clickedRowWasChecked;

    public FileHandlerForm()
    {
        InitializeComponent();
        Localizer.Localize(this);
        listView1.KeyDown += listView1_KeyDown;
        listView1.MouseDown += listView1_MouseDown;

        string[] drives = Directory.GetLogicalDrives();
        comboBox1.Items.AddRange(drives);
        if (drives.Length > 0)
        {
            string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "";
            int index = Array.FindIndex(drives, drive => drive.Equals(systemDrive, StringComparison.OrdinalIgnoreCase));
            comboBox1.SelectedIndex = Math.Max(index, 0); // loads the drive
        }
    } //constructor of File Handler form

    void ILocalizable.ApplyLocalization()
    {
        // Column headers are not controls, so the name-based lookup cannot reach them.
        nameCol.Text = Localizer.Get("FileHandlerForm.nameCol");
        extensionCol.Text = Localizer.Get("FileHandlerForm.extensionCol");
        sizeCol.Text = Localizer.Get("FileHandlerForm.sizeCol");
        dateCol.Text = Localizer.Get("FileHandlerForm.dateCol");
        fullPath.Text = Localizer.Get("FileHandlerForm.fullPath");
    }

    #region State

    /// <summary>What this panel is browsing.</summary>
    internal IFileSystem FileSystem => fileSystem;

    /// <summary>The folder shown in this panel.</summary>
    public string CurrentPath => currentPath;

    /// <summary>True while the panel shows an FTP server.</summary>
    public bool IsRemote => !fileSystem.IsLocal;

    /// <summary>The items to work on: the checked ones, or the selected ones when nothing is checked.</summary>
    internal IReadOnlyList<FileEntry> GetTargets()
    {
        var rows = listView1.CheckedItems.Count > 0
            ? listView1.CheckedItems.Cast<ListViewItem>()
            : listView1.SelectedItems.Cast<ListViewItem>();
        return [.. rows.Select(row => row.Tag).OfType<FileEntry>()];
    }

    /// <summary>Checks every item of the list.</summary>
    public void CheckAll()
    {
        listView1.BeginUpdate();
        foreach (ListViewItem row in listView1.Items)
        {
            row.Checked = row.Tag is FileEntry;
        }
        listView1.EndUpdate();
    }

    #endregion

    #region Loading

    /// <summary>Reloads the current folder (and leaves a search result list).</summary>
    public Task RefreshAsync() => LoadAsync(fileSystem, currentPath);

    /// <summary>Shows another folder of the current file system.</summary>
    public Task NavigateAsync(string path) => LoadAsync(fileSystem, path);

    /// <summary>Shows a folder of the local disks, leaving an FTP server if one was open.</summary>
    public async Task GoToLocalAsync(string path)
    {
        if (await SwitchAsync(LocalFileSystem.Instance, path))
        {
            SelectDrive(comboBox1.FindStringExact(Path.GetPathRoot(path) ?? ""));
        }
        else
        {
            SelectDrive(currentDriveIndex); // the drive box must keep showing where we really are
        }
    }

    /// <summary>Shows a folder of an FTP server. The panel takes over (and later disposes) <paramref name="remote"/>.</summary>
    internal async Task<bool> ConnectAsync(IFileSystem remote, string path)
    {
        if (!await SwitchAsync(remote, path))
        {
            return false;
        }
        SelectDrive(-1);
        return true;
    }

    /// <summary>Closes the FTP connection and goes back to the local disks.</summary>
    public Task DisconnectAsync() => GoToLocalAsync(Path.GetPathRoot(Environment.SystemDirectory) ?? @"C:\");

    private async Task<bool> SwitchAsync(IFileSystem target, string path)
    {
        IFileSystem previous = fileSystem;
        if (!await LoadAsync(target, path))
        {
            return false;
        }
        if (!ReferenceEquals(previous, fileSystem))
        {
            previous.Dispose();
        }
        return true;
    }

    private void SelectDrive(int index)
    {
        selectingDrive = true;
        comboBox1.SelectedIndex = index;
        selectingDrive = false;
        currentDriveIndex = index;
    }

    /// <summary>Lists <paramref name="path"/> and, only if that worked, makes it the current folder.</summary>
    private async Task<bool> LoadAsync(IFileSystem target, string path)
    {
        int version = ++loadVersion;
        listView1.UseWaitCursor = true;
        try
        {
            IReadOnlyList<FileEntry> entries;
            try
            {
                entries = await Task.Run(() => target.List(path));
            }
            catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
            {
                if (version == loadVersion && !IsDisposed)
                {
                    MessageBox.Show(this, ex is UnauthorizedAccessException ? Strings.AccessDenied : FileSystemErrors.Describe(ex));
                }
                return false;
            }

            if (version != loadVersion || IsDisposed)
            {
                return false; // the user has moved on in the meantime
            }

            fileSystem = target;
            currentPath = path;
            showingSearchResults = false;
            Text = target.GetDisplayPath(path);
            ShowEntries(entries);
            return true;
        }
        finally
        {
            if (version == loadVersion && !IsDisposed)
            {
                listView1.UseWaitCursor = false;
            }
        }
    }

    /// <summary>Replaces the listing with search results; the first row leads back to the folder.</summary>
    internal void ShowSearchResults(IReadOnlyList<FileEntry> results, string pattern)
    {
        loadVersion++;
        showingSearchResults = true;
        Text = $"{pattern} - {fileSystem.GetDisplayPath(currentPath)}";
        ShowEntries(results);
    }

    private void ShowEntries(IReadOnlyList<FileEntry> entries)
    {
        var rows = new ListViewItem[entries.Count + 1];
        rows[0] = new ListViewItem(ParentEntry);
        for (int i = 0; i < entries.Count; i++)
        {
            rows[i + 1] = CreateRow(entries[i]);
        }

        // One batch instead of a repaint per row.
        listView1.BeginUpdate();
        listView1.Items.Clear();
        listView1.Items.AddRange(rows);
        listView1.EndUpdate();
    }

    private static ListViewItem CreateRow(FileEntry entry)
    {
        string name = entry.Name;
        string type = "DIR";
        string size = " ";
        if (!entry.IsDirectory)
        {
            type = Path.GetExtension(entry.Name);
            name = type.Length < name.Length ? name[..^type.Length] : name; // ".gitignore" keeps its name
            size = ((entry.Size + 1023) / 1024).ToString("N0");
        }
        string date = entry.Modified == default ? "" : entry.Modified.ToString("g");

        return new ListViewItem([name, type, size, date, entry.FullPath])
        {
            Tag = entry,
            ImageIndex = entry.IsDirectory ? FolderImage : FileImage,
        };
    }

    #endregion

    #region Opening items

    /// <summary>Enters the selected folder, opens the selected file, or goes up for the first ("/.../") row.</summary>
    public async void ActivateSelected()
    {
        ListViewItem? row = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0] : listView1.FocusedItem;
        switch (row?.Tag)
        {
            case FileEntry { IsDirectory: true } folder:
                await NavigateAsync(folder.FullPath);
                break;
            case FileEntry file:
                OpenFile(file, inEditor: false);
                break;
            default:
                if (row is not null)
                {
                    await GoToParentAsync();
                }
                break;
        }
    }

    private Task GoToParentAsync()
    {
        if (showingSearchResults)
        {
            return RefreshAsync();
        }
        return fileSystem.GetParent(currentPath) is { } parent ? NavigateAsync(parent) : Task.CompletedTask;
    }

    /// <summary>
    /// Opens a file: text files (or anything when <paramref name="inEditor"/> is set) in the built-in
    /// editor, the rest with the application Windows associates with it. Files on a server are
    /// downloaded to a temporary folder first, so changes made to them are not sent back.
    /// </summary>
    internal void OpenFile(FileEntry file, bool inEditor)
    {
        string path = file.FullPath;
        if (!fileSystem.IsLocal)
        {
            string folder = Path.Combine(TempFile.Folder, Guid.NewGuid().ToString("N"));
            IFileSystem source = fileSystem;
            if (!ProcessForm.Run(this, file.Name, context =>
                    FileOperations.Transfer(context, source, [file], LocalFileSystem.Instance, folder, move: false)))
            {
                return;
            }
            path = Path.Combine(folder, file.Name);
        }

        try
        {
            if (inEditor || Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                new BuiltInTextEditor(path).Show();
            }
            else
            {
                // UseShellExecute defaults to false on modern .NET; it is required to open
                // documents with their associated application.
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
        }
        catch (Exception ex) when (ex is Win32Exception || FileSystemErrors.IsExpected(ex))
        {
            MessageBox.Show(this, ex.Message);
        }
    }

    #endregion

    #region FormHandling

    private async void comboBox1_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!selectingDrive && comboBox1.SelectedItem is string drive)
        {
            await GoToLocalAsync(drive);
        }
    }  //this is for the drive letter change

    private void listView1_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        // A double click also toggles the check box of the row; the click was meant to open it.
        if (clickedRow is not null && clickedRow.ListView == listView1)
        {
            clickedRow.Checked = clickedRowWasChecked;
        }
        ActivateSelected();
    } //this is for entering to a folder

    private void listView1_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Clicks == 1)
        {
            clickedRow = listView1.GetItemAt(e.X, e.Y);
            clickedRowWasChecked = clickedRow?.Checked ?? false;
        }
    }

    private void listView1_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            e.Handled = true;
            ActivateSelected();
        }
    }  //this is for using enter button

    private async void listView1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Back)
        {
            e.Handled = true;
            await GoToParentAsync();
        }
    }

    private void FileHandlerForm_Enter(object? sender, EventArgs e)
    {
        pictureBox1.Visible = true;
        pictureBox2.Visible = false;
    }  //change picture state

    private void FileHandlerForm_Leave(object? sender, EventArgs e)
    {
        pictureBox1.Visible = false;
        pictureBox2.Visible = true;
    } //change picture state

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        loadVersion++;
        fileSystem.Dispose();
    }

    #endregion
}
