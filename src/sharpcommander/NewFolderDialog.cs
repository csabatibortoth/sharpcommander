using sharpcommander.Localization;

namespace sharpcommander;

/// <summary>Asks for a name: by default the name of a new folder, optionally anything else (rename).</summary>
public partial class NewFolderDialog : Form
{
    public NewFolderDialog()
    {
        InitializeComponent();
        Localizer.Localize(this);
        AcceptButton = okBtn;
        StartPosition = FormStartPosition.CenterParent;
    }

    /// <param name="caption">Window title.</param>
    /// <param name="prompt">Label in front of the text box.</param>
    /// <param name="initialName">The name to start from; its base name (without extension) is preselected.</param>
    public NewFolderDialog(string caption, string prompt, string initialName) : this()
    {
        Text = caption;
        label1.Text = prompt;
        textBox1.Text = initialName;

        // Keep the text box clear of a longer label.
        int left = label1.Right + 6;
        if (left > textBox1.Left)
        {
            textBox1.SetBounds(left, textBox1.Top, textBox1.Right - left, textBox1.Height);
        }

        int extension = initialName.LastIndexOf('.');
        textBox1.Select(0, extension > 0 ? extension : initialName.Length);
    }

    /// <summary>The name that was typed, without surrounding spaces.</summary>
    public string BackFolderName => textBox1.Text.Trim();

    private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            e.Handled = true; // the default button already accepts; just avoid the beep
        }
    } //exit to enter button
}
