using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace sharpcommander;

public partial class FileHandlerForm : Form
{
    private const string ParentEntry = "/.../";

    public FileHandlerForm()
    {
        InitializeComponent();

        int index = 0;
        foreach (string drive in Directory.GetLogicalDrives())
        {
            comboBox1.Items.Add(drive);
            if (drive == "C:\\")
            {
                index = comboBox1.Items.IndexOf(drive);
            }
        }
        comboBox1.SelectedIndex = index;
    } //constructor of File Handler form

    #region FormHandling

    public void PopulateList()
    {
        listView1.Items.Clear();
        listView1.Items.Add(ParentEntry);
        try
        {
            foreach (string directory in Directory.GetDirectories(Text))
            {
                AddEntry(directory, "DIR", " ");
            }
            foreach (string file in Directory.GetFiles(Text))
            {
                AddEntry(file, Path.GetExtension(file), FileSizeText(file));
            }
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show("Nincs megfelelő joga a mappa megnyitásához!");
            Text = SelectedDrive;
            PopulateList();
        }
        catch (IOException iex)
        {
            MessageBox.Show(iex.Message);
            comboBox1.SelectedIndex = 0;
        }
    } //this method used to populate listview

    private void AddEntry(string path, string type, string size)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        string created = Directory.GetCreationTimeUtc(path).ToString();
        listView1.Items.Add(name).SubItems.AddRange([type, size, created, path]);
    }

    private static string FileSizeText(string path)
    {
        try
        {
            return new FileInfo(path).Length.ToString();
        }
        catch (FileNotFoundException)
        {
            return " ";
        }
    }

    private string SelectedDrive => comboBox1.SelectedItem?.ToString() ?? "";

    private void RunApp(string path)
    {
        switch (Path.GetExtension(path))
        {
            case ".txt":
                OpenTxt(path);
                break;

            default:
                // UseShellExecute defaults to false on modern .NET; it is required to open
                // documents with their associated application.
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                break;
        }
    } //run application for proper extensions

    private static void OpenTxt(string source)
    {
        new BuiltInTextEditor(ReadTextLines(source)).Show();
    } //this method for opening .txt files

    /// <summary>
    /// Reads a text file as UTF-8 (honouring byte order marks) and falls back to the system
    /// ANSI code page, which is what <c>Encoding.Default</c> meant on .NET Framework.
    /// </summary>
    private static string[] ReadTextLines(string path)
    {
        try
        {
            return File.ReadAllLines(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        }
        catch (DecoderFallbackException)
        {
            Encoding ansi = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
            return File.ReadAllLines(path, ansi);
        }
    }

    private void comboBox1_SelectedIndexChanged(object? sender, EventArgs e)
    {
        Text = SelectedDrive;
        PopulateList();
    }  //this is for the drive letter change

    private void listView1_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        ActivateSelectedItem();
    } //this is for entering to a folder

    private void listView1_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            ActivateSelectedItem();
        }
    }  //this is for using enter button

    /// <summary>Enters the selected folder, opens the selected file, or goes up for the first ("/.../") row.</summary>
    private void ActivateSelectedItem()
    {
        if (listView1.SelectedItems is not [var item, ..])
        {
            PopulateList();
            return;
        }

        if (item.Index == 0)
        {
            GoToParent();
        }
        else if (item.SubItems[1].Text == "DIR")
        {
            Text = item.SubItems[4].Text;
            PopulateList();
        }
        else
        {
            RunApp(item.SubItems[4].Text);
        }
    }

    private void GoToParent()
    {
        Text = FileAcces.backparent(Text);
        PopulateList();
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

    #endregion

    #region CommunicateOtherObjects

    /// <summary>Returns true when at least one listview item is checked.</summary>
    public bool IsChecked() => listView1.CheckedItems.Count != 0;

    /// <summary>The path of the current folder.</summary>
    public string BackPath => Text;

    /// <summary>The full paths of the checked items.</summary>
    public string[] BackTrails() =>
        [.. listView1.CheckedItems.Cast<ListViewItem>().Select(item => item.SubItems[4].Text)];

    private void listView1_SelectedIndexChanged(object? sender, EventArgs e)
    {
    }

    #endregion
}
