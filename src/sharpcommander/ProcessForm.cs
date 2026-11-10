using sharpcommander.Localization;

namespace sharpcommander;

public partial class ProcessForm : Form
{
    public ProcessForm()
    {
        InitializeComponent();
        Localizer.Localize(this);
    }
}
