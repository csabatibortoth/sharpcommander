namespace sharpcommander.FileSystems;

/// <summary>The local disks. Stateless, so one shared instance serves every panel.</summary>
internal sealed class LocalFileSystem : IFileSystem
{
    public static LocalFileSystem Instance { get; } = new();

    private const int CopyBufferSize = 1024 * 1024;

    private LocalFileSystem()
    {
    }

    public bool IsLocal => true;

    public StringComparison PathComparison => StringComparison.OrdinalIgnoreCase;

    public string GetDisplayPath(string path) => path;

    public bool IsSameStore(IFileSystem other) => other is LocalFileSystem;

    public IReadOnlyList<FileEntry> List(string path)
    {
        // One enumeration returns name, size and dates together; no extra disk access per file.
        var directory = new DirectoryInfo(path);
        return
        [
            .. directory.EnumerateDirectories().OrderBy(d => d.Name, StringComparer.CurrentCultureIgnoreCase).Select(ToEntry),
            .. directory.EnumerateFiles().OrderBy(f => f.Name, StringComparer.CurrentCultureIgnoreCase).Select(ToEntry),
        ];
    }

    /// <summary>Describes an existing file or folder, or returns <c>null</c> if there is none at <paramref name="path"/>.</summary>
    public static FileEntry? GetEntry(string path)
    {
        if (Directory.Exists(path))
        {
            return ToEntry(new DirectoryInfo(path));
        }
        return File.Exists(path) ? ToEntry(new FileInfo(path)) : null;
    }

    private static FileEntry ToEntry(FileSystemInfo info) =>
        new(info.Name, info.FullName, info is DirectoryInfo, info is FileInfo file ? file.Length : 0, info.LastWriteTime,
            IsLink: info.Attributes.HasFlag(FileAttributes.ReparsePoint) && info.LinkTarget is not null);

    public string? GetParent(string path) => Directory.GetParent(path)?.FullName;

    public string Combine(string path, string name) => Path.Combine(path, name);

    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteFile(string path)
    {
        ClearReadOnly(new FileInfo(path));
        File.Delete(path);
    }

    public void DeleteDirectory(string path)
    {
        var directory = new DirectoryInfo(path);
        if (!directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            // Read-only files would make the recursive delete fail half-way.
            var options = new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.None };
            foreach (FileSystemInfo item in directory.EnumerateFileSystemInfos("*", options))
            {
                ClearReadOnly(item);
            }
        }
        ClearReadOnly(directory);
        directory.Delete(recursive: true);
    }

    private static void ClearReadOnly(FileSystemInfo item)
    {
        if (item.Exists && item.Attributes.HasFlag(FileAttributes.ReadOnly))
        {
            item.Attributes &= ~FileAttributes.ReadOnly;
        }
    }

    public void Move(string source, string destination)
    {
        if (Directory.Exists(source))
        {
            Directory.Move(source, destination);
        }
        else
        {
            File.Move(source, destination);
        }
    }

    public void Download(string path, Stream target)
    {
        using FileStream source = OpenRead(path);
        source.CopyTo(target, CopyBufferSize);
    }

    public void Upload(Stream source, string path)
    {
        using FileStream target = OpenWrite(path);
        source.CopyTo(target, CopyBufferSize);
    }

    public static FileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize, FileOptions.SequentialScan);

    public static FileStream OpenWrite(string path) =>
        new(path, FileMode.Create, FileAccess.Write, FileShare.None, CopyBufferSize, FileOptions.SequentialScan);

    public void Dispose()
    {
    }
}
