using System.ComponentModel;

namespace sharpcommander;

public partial class PathVerifyForm : Form
{
    // Runtime-only state: must not be serialized into designer code.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Targettrails { get; set; }

    public PathVerifyForm(string[] sourcetrails, string targettrail)
    {
        InitializeComponent();
        Targettrails = targettrail;
        textBox2.Text = targettrail;
        textBox1.Text = sourcetrails is [var single] ? single : @"\*";
    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {
        Targettrails = textBox2.Text;
    }
}
