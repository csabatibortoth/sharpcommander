namespace sharpcommander;

public partial class AreYouSureForm : Form
{
    public AreYouSureForm(string text)
    {
        InitializeComponent();
        label1.Text = text;
    }
}
