using System.ComponentModel;
using System.Drawing.Printing;
using System.Globalization;
using System.Text;
using sharpcommander.FileSystems;
using sharpcommander.Localization;

namespace sharpcommander;

/// <summary>A small plain-text editor.</summary>
public partial class BuiltInTextEditor : Form, ILocalizable
{
    private readonly PrintDocument printDocument = new();
    private string printText = "";
    private int printOffset;

    private string? filePath;
    private Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private string lineEnding = Environment.NewLine;

    /// <param name="path">The file to open, or <c>null</c> to start with an empty document.</param>
    public BuiltInTextEditor(string? path = null)
    {
        InitializeComponent();
        richTextBox1.DetectUrls = false;
        richTextBox1.AcceptsTab = true;
        richTextBox1.Font = new Font("Consolas", 10F);
        richTextBox1.ModifiedChanged += (_, _) => UpdateTitle();
        richTextBox1.TextChanged += (_, _) => UpdateTitle(); // not every kind of edit raises ModifiedChanged
        WireCommands();
        Localizer.Localize(this);

        if (path is not null)
        {
            LoadFile(path);
        }
    }

    void ILocalizable.ApplyLocalization() => UpdateTitle();

    private void WireCommands()
    {
        void Bind(ToolStripMenuItem menuItem, ToolStripItem? button, Action command, Keys shortcut = Keys.None)
        {
            menuItem.Click += (_, _) => command();
            menuItem.ShortcutKeys = shortcut;
            if (button is not null)
            {
                button.Click += (_, _) => command();
            }
        }

        Bind(newToolStripMenuItem, newToolStripButton, NewDocument, Keys.Control | Keys.N);
        Bind(openToolStripMenuItem, openToolStripButton, OpenDocument, Keys.Control | Keys.O);
        Bind(saveToolStripMenuItem, saveToolStripButton, () => Save(), Keys.Control | Keys.S);
        Bind(saveAsToolStripMenuItem, null, () => SaveAs());
        Bind(printToolStripMenuItem, printToolStripButton, Print, Keys.Control | Keys.P);
        Bind(printPreviewToolStripMenuItem, null, PrintPreview);
        Bind(exitToolStripMenuItem, null, Close);

        Bind(undoToolStripMenuItem, null, richTextBox1.Undo, Keys.Control | Keys.Z);
        Bind(redoToolStripMenuItem, null, richTextBox1.Redo, Keys.Control | Keys.Y);
        Bind(cutToolStripMenuItem, cutToolStripButton, richTextBox1.Cut, Keys.Control | Keys.X);
        Bind(copyToolStripMenuItem, copyToolStripButton, richTextBox1.Copy, Keys.Control | Keys.C);
        Bind(pasteToolStripMenuItem, pasteToolStripButton, PastePlainText, Keys.Control | Keys.V);
        Bind(selectAllToolStripMenuItem, null, richTextBox1.SelectAll, Keys.Control | Keys.A);

        Bind(aboutToolStripMenuItem, helpToolStripButton, ShowAbout);

        // Left over from the menu template; this editor has no options and no help file.
        toolsToolStripMenuItem.Visible = false;
        contentsToolStripMenuItem.Visible = false;
        indexToolStripMenuItem.Visible = false;
        searchToolStripMenuItem.Visible = false;
        toolStripSeparator5.Visible = false;

        printDocument.BeginPrint += (_, _) =>
        {
            printText = richTextBox1.Text;
            printOffset = 0;
        };
        printDocument.PrintPage += PrintPage;
    }

    #region Document

    private string DocumentName => filePath is null ? Strings.EditorUntitled : Path.GetFileName(filePath);

    private void UpdateTitle()
    {
        Text = Strings.EditorTitle(DocumentName + (richTextBox1.Modified ? "*" : ""));
    }

    private void LoadFile(string path)
    {
        try
        {
            (string text, encoding) = TextFile.Read(path);
            lineEnding = text.Contains("\r\n") || !text.Contains('\n') ? "\r\n" : "\n";
            richTextBox1.Text = text;
            filePath = path;
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        richTextBox1.Modified = false;
        UpdateTitle();
    }

    private void NewDocument()
    {
        if (!ConfirmDiscard())
        {
            return;
        }

        richTextBox1.Clear();
        filePath = null;
        richTextBox1.Modified = false;
        UpdateTitle();
    }

    private void OpenDocument()
    {
        if (!ConfirmDiscard())
        {
            return;
        }

        using var dialog = new OpenFileDialog { Filter = Strings.EditorFilter };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFile(dialog.FileName);
        }
    }

    /// <returns>False if the document was not saved (cancelled or failed).</returns>
    private bool Save() => filePath is null ? SaveAs() : SaveTo(filePath);

    private bool SaveAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = Strings.EditorFilter,
            FileName = filePath is null ? "" : Path.GetFileName(filePath),
            InitialDirectory = filePath is null ? "" : Path.GetDirectoryName(filePath),
        };
        return dialog.ShowDialog(this) == DialogResult.OK && SaveTo(dialog.FileName);
    }

    private bool SaveTo(string path)
    {
        try
        {
            // The text box works with "\n"; write the line endings the file came with.
            File.WriteAllText(path, richTextBox1.Text.ReplaceLineEndings(lineEnding), encoding);
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        filePath = path;
        richTextBox1.Modified = false;
        UpdateTitle();
        return true;
    }

    /// <summary>Offers to save unsaved changes. Returns false if the user wants to stay in the document.</summary>
    private bool ConfirmDiscard()
    {
        if (!richTextBox1.Modified)
        {
            return true;
        }

        return MessageBox.Show(this, Strings.EditorSaveChanges(DocumentName), Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) switch
        {
            DialogResult.Yes => Save(),
            DialogResult.No => true,
            _ => false,
        };
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!ConfirmDiscard())
        {
            e.Cancel = true;
        }
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        printDocument.Dispose();
    }

    #endregion

    #region Commands

    private void PastePlainText()
    {
        // Text copied from a browser or a word processor must not bring its formatting along.
        if (Clipboard.ContainsText())
        {
            richTextBox1.SelectedText = Clipboard.GetText();
            richTextBox1.Modified = true;
        }
    }

    private void ShowAbout()
    {
        using var about = new ProgramInfoBox();
        about.ShowDialog(this);
    }

    private void Print()
    {
        using var dialog = new PrintDialog { Document = printDocument, UseEXDialog = true };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            printDocument.DocumentName = DocumentName;
            printDocument.Print();
        }
        catch (Exception ex) when (ex is InvalidPrinterException or Win32Exception)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void PrintPreview()
    {
        try
        {
            using var dialog = new PrintPreviewDialog { Document = printDocument };
            dialog.ShowDialog(this);
        }
        catch (Exception ex) when (ex is InvalidPrinterException or Win32Exception)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void PrintPage(object sender, PrintPageEventArgs e)
    {
        // Fill the page with as much text as fits; the rest goes to the next page.
        using var format = new StringFormat(StringFormatFlags.LineLimit);
        string remaining = printText[printOffset..];
        e.Graphics!.MeasureString(remaining, richTextBox1.Font, e.MarginBounds.Size, format, out int characters, out _);
        e.Graphics.DrawString(remaining[..characters], richTextBox1.Font, Brushes.Black, e.MarginBounds, format);

        printOffset += characters;
        e.HasMorePages = characters > 0 && printOffset < printText.Length;
    }

    #endregion
}

/// <summary>Reads text files whose encoding is not known in advance.</summary>
internal static class TextFile
{
    /// <summary>
    /// Returns the text and the encoding to save it back with. Byte order marks are honoured;
    /// without one the file is UTF-8 if it is valid UTF-8, otherwise it is taken to be in the
    /// system ANSI code page (what <c>Encoding.Default</c> meant on .NET Framework).
    /// </summary>
    public static (string Text, Encoding Encoding) Read(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);

        Encoding[] withMark = [new UTF8Encoding(true), new UnicodeEncoding(false, true), new UnicodeEncoding(true, true)];
        foreach (Encoding candidate in withMark)
        {
            ReadOnlySpan<byte> mark = candidate.Preamble;
            if (bytes.AsSpan().StartsWith(mark))
            {
                return (candidate.GetString(bytes, mark.Length, bytes.Length - mark.Length), candidate);
            }
        }

        try
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            return (utf8.GetString(bytes), new UTF8Encoding(false));
        }
        catch (DecoderFallbackException)
        {
            Encoding ansi = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
            return (ansi.GetString(bytes), ansi);
        }
    }
}
