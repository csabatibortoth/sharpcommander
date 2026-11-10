using sharpcommander.Localization;

namespace sharpcommander;

public partial class AreYouSureForm : Form
{
    public AreYouSureForm(string text)
    {
        InitializeComponent();
        Localizer.Localize(this);
        label1.Text = text;
    }
}
