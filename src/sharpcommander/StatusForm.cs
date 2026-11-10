using sharpcommander.Localization;

namespace sharpcommander;

public partial class StatusForm : Form
{
    public StatusForm()
    {
        InitializeComponent();
        Localizer.Localize(this);
        progressBar1.MarqueeAnimationSpeed = 30;
        progressBar1.Style = ProgressBarStyle.Marquee;
    }

    private void cancelBtn_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
    }
}
