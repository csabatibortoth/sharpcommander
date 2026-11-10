using sharpcommander.Localization;

namespace sharpcommander;

/// <summary>
/// A yes / no question. The result is <see cref="DialogResult.OK"/> for "yes",
/// <see cref="DialogResult.Yes"/> for "yes to all" and <see cref="DialogResult.Cancel"/> for "no".
/// </summary>
public partial class AreYouSureForm : Form
{
    /// <param name="offerAll">Show the "All" button, for questions that will come up repeatedly.</param>
    public AreYouSureForm(string text, bool offerAll = false)
    {
        InitializeComponent();
        Localizer.Localize(this);
        label1.Text = text;
        button1.Visible = offerAll;

        // The dialog was sized for one short sentence; grow it for longer questions (e.g. full paths).
        int needed = label1.Right + label1.Left;
        if (needed > ClientSize.Width)
        {
            ClientSize = new Size(needed, ClientSize.Height);
        }
        StartPosition = FormStartPosition.CenterParent;
    }

    /// <summary>Asks a plain yes / no question.</summary>
    public static bool Ask(IWin32Window owner, string question)
    {
        using var dialog = new AreYouSureForm(question);
        return dialog.ShowDialog(owner) == DialogResult.OK;
    }
}
