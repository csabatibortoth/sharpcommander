namespace sharpcommander.Localization;

/// <summary>
/// Typed access to the strings that are used from code. The texts live in <c>Strings.resx</c>
/// (Hungarian, neutral) and <c>Strings.&lt;culture&gt;.resx</c>; control texts are applied by
/// <see cref="Localizer.Localize"/> and need no entry here.
/// </summary>
internal static class Strings
{
    public static string ConfirmOverwriteItem(string path) => Localizer.Format("Msg.ConfirmOverwriteItem", path);
    public static string ConfirmOverwriteFiles => Localizer.Get("Msg.ConfirmOverwriteFiles");
    public static string ConfirmOverwriteFolder => Localizer.Get("Msg.ConfirmOverwriteFolder");
    public static string ConfirmDelete => Localizer.Get("Msg.ConfirmDelete");
    public static string NothingSelected => Localizer.Get("Msg.NothingSelected");
    public static string CannotCompressFolders => Localizer.Get("Msg.CannotCompressFolders");
    public static string DownloadFinished => Localizer.Get("Msg.DownloadFinished");
    public static string UnderDevelopment => Localizer.Get("Msg.UnderDevelopment");
    public static string SearchPrompt => Localizer.Get("Msg.SearchPrompt");
    public static string UpToDate => Localizer.Get("Msg.UpToDate");
    public static string UpdateCheckFailed => Localizer.Get("Msg.UpdateCheckFailed");
    public static string AccessDenied => Localizer.Get("Msg.AccessDenied");
    public static string ZipFilter => Localizer.Get("Msg.ZipFilter");

    public static string CopyFinished => Localizer.Get("Msg.CopyFinished");
    public static string MoveFinished => Localizer.Get("Msg.MoveFinished");
    public static string DeleteFinished => Localizer.Get("Msg.DeleteFinished");
    public static string CompressFinished => Localizer.Get("Msg.CompressFinished");

    public static string StatusCopying => Localizer.Get("Status.Copying");
    public static string StatusMoving => Localizer.Get("Status.Moving");
    public static string StatusDeleting => Localizer.Get("Status.Deleting");
    public static string StatusCompressing => Localizer.Get("Status.Compressing");

    public static string EditorTitle => Localizer.Get("Editor.Title");
    public static string AboutTitle(string product) => Localizer.Format("About.Title", product);
    public static string AboutVersion(string version) => Localizer.Format("About.Version", version);
}
