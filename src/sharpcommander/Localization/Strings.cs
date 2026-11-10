namespace sharpcommander.Localization;

/// <summary>
/// Typed access to the strings that are used from code. The texts live in <c>Strings.resx</c>
/// (Hungarian, neutral) and <c>Strings.&lt;culture&gt;.resx</c>; control texts are applied by
/// <see cref="Localizer.Localize"/> and need no entry here.
/// </summary>
internal static class Strings
{
    public static string ConfirmOverwriteItem(string path) => Localizer.Format("Msg.ConfirmOverwriteItem", path);
    public static string ConfirmDelete => Localizer.Get("Msg.ConfirmDelete");
    public static string NothingSelected => Localizer.Get("Msg.NothingSelected");
    public static string DownloadFinished => Localizer.Get("Msg.DownloadFinished");
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

    public static string AboutTitle(string product) => Localizer.Format("About.Title", product);
    public static string AboutVersion(string version) => Localizer.Format("About.Version", version);

    public static string StatusScanning => Localizer.Get("Status.Scanning");
    public static string StatusExtracting => Localizer.Get("Status.Extracting");
    public static string StatusSearching => Localizer.Get("Status.Searching");
    public static string StatusConnecting(string host) => Localizer.Format("Status.Connecting", host);
    public static string ExtractFinished => Localizer.Get("Msg.ExtractFinished");

    public static string SameSourceAndTarget(string name) => Localizer.Format("Msg.SameSourceAndTarget", name);
    public static string UnsupportedArchive(string name) => Localizer.Format("Msg.UnsupportedArchive", name);
    public static string UnsafeArchiveEntry(string name) => Localizer.Format("Msg.UnsafeArchiveEntry", name);
    public static string AlreadyExists(string name) => Localizer.Format("Msg.AlreadyExists", name);
    public static string RenameSingleItem => Localizer.Get("Msg.RenameSingleItem");
    public static string RenameTitle => Localizer.Get("Rename.Title");
    public static string RenamePrompt => Localizer.Get("Rename.Prompt");
    public static string SearchPatternRequired => Localizer.Get("Msg.SearchPatternRequired");
    public static string NoSearchResults(string pattern) => Localizer.Format("Msg.NoSearchResults", pattern);
    public static string ClipboardLocalOnly => Localizer.Get("Msg.ClipboardLocalOnly");

    public static string FtpSecurityAuto => Localizer.Get("Ftp.SecurityAuto");
    public static string FtpSecurityNone => Localizer.Get("Ftp.SecurityNone");
    public static string FtpSecurityExplicit => Localizer.Get("Ftp.SecurityExplicit");
    public static string FtpSecurityImplicit => Localizer.Get("Ftp.SecurityImplicit");
    public static string FtpHostRequired => Localizer.Get("Ftp.HostRequired");
    public static string FtpUntrustedCertificate(string details) => Localizer.Format("Ftp.UntrustedCertificate", details);

    public static string EditorTitle(string document) => Localizer.Format("Editor.Title", document);
    public static string EditorUntitled => Localizer.Get("Editor.Untitled");
    public static string EditorSaveChanges(string document) => Localizer.Format("Editor.SaveChanges", document);
    public static string EditorFilter => Localizer.Get("Editor.Filter");
}
