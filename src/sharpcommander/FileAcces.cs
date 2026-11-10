namespace sharpcommander;

internal class FileAcces
{
    private readonly string[]? trails;

    public FileAcces(string path)
    {
        try
        {
            trails = Directory.GetFileSystemEntries(path);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            MessageBox.Show(ex.Message);
        }
    }

    public string[]? backpath() => trails;

    /// <summary>Returns the parent folder of <paramref name="path"/>, or the path itself for a drive root.</summary>
    public static string backparent(string path) => Directory.GetParent(path)?.FullName ?? path;
}
