namespace sharpcommander;

public partial class BuiltInTextEditor : Form
{
    public BuiltInTextEditor(string[]? text)
    {
        InitializeComponent();
        richTextBox1.Lines = text;
        Text = "Sharp Editor 0.1 (Csak Megtekintés)";
    }
}
