using sharpcommander.Localization;

namespace sharpcommander;

public partial class NewFolderDialog : Form
{
    public string BackFolderName => textBox1.Text;

    public NewFolderDialog()
    {
        InitializeComponent();
        Localizer.Localize(this);
    }

    private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            DialogResult = DialogResult.OK;
        }
    } //exit to enter button
}
