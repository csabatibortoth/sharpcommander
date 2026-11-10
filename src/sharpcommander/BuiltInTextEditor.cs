using sharpcommander.Localization;

namespace sharpcommander;

public partial class BuiltInTextEditor : Form, ILocalizable
{
    public BuiltInTextEditor(string[]? text)
    {
        InitializeComponent();
        richTextBox1.Lines = text;
        Localizer.Localize(this);
    }

    void ILocalizable.ApplyLocalization()
    {
        Text = Strings.EditorTitle;
    }
}
