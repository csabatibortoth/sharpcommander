using System.Text;

namespace sharpcommander;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // Legacy ANSI code pages (e.g. windows-1250) are not built into modern .NET;
        // the text viewer needs them for files that are not UTF-8.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Visual styles, text rendering, DPI mode and default font come from the project file.
        ApplicationConfiguration.Initialize();
        Application.Run(new mainForm());
    }
}
