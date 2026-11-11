using System.Reflection;
using sharpcommander.Localization;

namespace sharpcommander;

partial class ProgramInfoBox : Form, ILocalizable
{
    private static readonly Assembly ThisAssembly = typeof(ProgramInfoBox).Assembly;

    public ProgramInfoBox()
    {
        InitializeComponent();
        labelProductName.Text = AssemblyProduct;
        labelCopyright.Text = AssemblyCopyright;
        labelCompanyName.Text = AssemblyCompany;
        textBoxDescription.Text = AssemblyDescription;
        Localizer.Localize(this);
    }

    void ILocalizable.ApplyLocalization()
    {
        Text = Strings.AboutTitle(AssemblyTitle);
        labelVersion.Text = Strings.AboutVersion(AssemblyVersion);
    }

    #region Assembly Attribute Accessors

    public string AssemblyTitle =>
        ThisAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title is { Length: > 0 } title
            ? title
            : ThisAssembly.GetName().Name ?? "";

    public string AssemblyVersion => ThisAssembly.GetName().Version?.ToString() ?? "";

    public string AssemblyDescription =>
        ThisAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";

    public string AssemblyProduct =>
        ThisAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "";

    public string AssemblyCopyright =>
        ThisAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";

    public string AssemblyCompany =>
        ThisAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "";

    #endregion
}
