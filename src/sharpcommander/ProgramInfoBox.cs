using System.Reflection;

namespace sharpcommander;

partial class ProgramInfoBox : Form
{
    private static readonly Assembly ThisAssembly = typeof(ProgramInfoBox).Assembly;

    public ProgramInfoBox()
    {
        InitializeComponent();
        Text = $"About {AssemblyTitle}";
        labelProductName.Text = AssemblyProduct;
        labelVersion.Text = $"Version {AssemblyVersion}";
        labelCopyright.Text = AssemblyCopyright;
        labelCompanyName.Text = AssemblyCompany;
        textBoxDescription.Text = AssemblyDescription;
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
