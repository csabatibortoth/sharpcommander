using System.Net.Sockets;
using System.Security.Authentication;
using FluentFTP.Exceptions;

namespace sharpcommander.FileSystems;

/// <summary>One row of a directory listing.</summary>
internal sealed record FileEntry(string Name, string FullPath, bool IsDirectory, long Size, DateTime Modified, bool IsLink = false);

/// <summary>
/// What a file panel can browse and what file operations work on: the local disks or an FTP server.
/// All members are blocking and may be called from a background thread.
/// </summary>
internal interface IFileSystem : IDisposable
{
    /// <summary>True for the local disks: paths can then be used with <see cref="File"/> directly.</summary>
    bool IsLocal { get; }

    /// <summary>How paths of this file system compare (Windows paths ignore case, FTP paths do not).</summary>
    StringComparison PathComparison { get; }

    /// <summary>Text for the panel caption.</summary>
    string GetDisplayPath(string path);

    /// <summary>True when <paramref name="other"/> addresses the same storage, so a path means the same thing in both.</summary>
    bool IsSameStore(IFileSystem other);

    /// <summary>Lists a folder: folders first, each group sorted by name.</summary>
    IReadOnlyList<FileEntry> List(string path);

    /// <summary>The parent folder, or <c>null</c> for a root.</summary>
    string? GetParent(string path);

    string Combine(string path, string name);

    bool FileExists(string path);

    bool DirectoryExists(string path);

    /// <summary>Creates a folder (and missing parents); does nothing if it already exists.</summary>
    void CreateDirectory(string path);

    void DeleteFile(string path);

    /// <summary>Deletes a folder with everything in it.</summary>
    void DeleteDirectory(string path);

    /// <summary>Renames or moves a file or folder inside this file system.</summary>
    void Move(string source, string destination);

    /// <summary>Writes the content of the file at <paramref name="path"/> into <paramref name="target"/>.</summary>
    void Download(string path, Stream target);

    /// <summary>Creates or overwrites the file at <paramref name="path"/> from <paramref name="source"/>.</summary>
    void Upload(Stream source, string path);
}

internal static class FileSystemErrors
{
    /// <summary>
    /// Failures that are part of normal life when working with files and servers (no access,
    /// file in use, connection lost, ...). They are reported to the user instead of crashing.
    /// </summary>
    public static bool IsExpected(Exception ex) => ex
        is IOException
        or UnauthorizedAccessException
        or ArgumentException
        or NotSupportedException
        or System.Security.SecurityException
        or FtpException
        or SocketException
        or TimeoutException
        or AuthenticationException
        or InvalidDataException;

    /// <summary>The most specific message of an exception chain (FluentFTP wraps the server's reply).</summary>
    public static string Describe(Exception ex)
    {
        while (ex.InnerException is { } inner && (ex is AggregateException || ex.InnerException is FtpException))
        {
            ex = inner;
        }
        return ex.Message;
    }
}
