using System.IO.Compression;
using System.IO.Enumeration;
using sharpcommander.FileSystems;
using sharpcommander.Localization;

namespace sharpcommander.Operations;

/// <summary>
/// The file operations of the commander. They work between any two <see cref="IFileSystem"/>s,
/// run on a background thread and talk to the user only through the <see cref="OperationContext"/>.
/// </summary>
internal static class FileOperations
{
    private const int BufferSize = 1024 * 1024;

    /// <summary>One step of a copy: a source item and where it goes.</summary>
    private sealed record PlanNode(FileEntry Source, string Target);

    #region Copy / move

    public static void Transfer(
        OperationContext context,
        IFileSystem source,
        IReadOnlyList<FileEntry> items,
        IFileSystem destination,
        string destinationFolder,
        bool move)
    {
        bool sameStore = source.IsSameStore(destination);
        destination.CreateDirectory(destinationFolder);

        var toCopy = new List<FileEntry>();
        foreach (FileEntry item in items)
        {
            string target = destination.Combine(destinationFolder, item.Name);
            if (sameStore)
            {
                EnsureDistinct(source, item, target);
            }

            // Moving inside one disk or server is a rename: instant, whatever the size.
            context.Status(item.Name);
            if (!(move && sameStore && TryRename(source, destination, item, target)))
            {
                toCopy.Add(item);
            }
        }

        if (toCopy.Count == 0)
        {
            return;
        }

        context.Status(Strings.StatusScanning);
        var plan = new List<PlanNode>();
        foreach (FileEntry item in toCopy)
        {
            Scan(context, source, item, destination.Combine(destinationFolder, item.Name), destination.Combine, plan);
        }
        SetTotals(context, plan);

        foreach (PlanNode node in plan)
        {
            if (node.Source.IsDirectory)
            {
                context.Token.ThrowIfCancellationRequested();
                destination.CreateDirectory(node.Target);
            }
            else if (CopyFile(context, source, node, destination) && move)
            {
                source.DeleteFile(node.Source.FullPath);
            }
        }

        if (move)
        {
            RemoveEmptiedFolders(source, plan);
        }
    }

    private static bool TryRename(IFileSystem source, IFileSystem destination, FileEntry item, string target)
    {
        if (destination.FileExists(target) || destination.DirectoryExists(target))
        {
            return false; // needs the overwrite question and a merge: take the copy route
        }

        try
        {
            source.Move(item.FullPath, target);
            return true;
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            return false; // e.g. a folder going to another drive: copy and delete instead
        }
    }

    /// <summary>Refuses to copy something onto itself or a folder into its own subtree.</summary>
    private static void EnsureDistinct(IFileSystem fileSystem, FileEntry item, string target)
    {
        if (string.Equals(item.FullPath, target, fileSystem.PathComparison)
            || (item.IsDirectory && IsInside(fileSystem, item.FullPath, target)))
        {
            throw new IOException(Strings.SameSourceAndTarget(item.Name));
        }
    }

    private static bool IsInside(IFileSystem fileSystem, string folder, string path)
    {
        string prefix = fileSystem.Combine(folder, "x")[..^1]; // the folder plus its separator
        return path.StartsWith(prefix, fileSystem.PathComparison);
    }

    /// <summary>Adds <paramref name="item"/> and, for folders, everything below it to the plan (parents first).</summary>
    private static void Scan(
        OperationContext context,
        IFileSystem source,
        FileEntry item,
        string target,
        Func<string, string, string> combine,
        List<PlanNode> plan)
    {
        context.Token.ThrowIfCancellationRequested();
        plan.Add(new PlanNode(item, target));
        if (!item.IsDirectory)
        {
            return;
        }

        foreach (FileEntry child in source.List(item.FullPath))
        {
            Scan(context, source, child, combine(target, child.Name), combine, plan);
        }
    }

    private static void SetTotals(OperationContext context, List<PlanNode> plan)
    {
        int files = 0;
        long bytes = 0;
        foreach (PlanNode node in plan)
        {
            if (!node.Source.IsDirectory)
            {
                files++;
                bytes += node.Source.Size;
            }
        }
        context.SetTotals(files, bytes);
    }

    /// <returns>False when the user chose not to overwrite an existing file.</returns>
    private static bool CopyFile(OperationContext context, IFileSystem source, PlanNode node, IFileSystem destination)
    {
        context.BeginItem(node.Source.Name, node.Source.Size);
        if (destination.FileExists(node.Target) && !context.ConfirmOverwrite(destination.GetDisplayPath(node.Target)))
        {
            context.EndItem();
            return false;
        }

        if (source.IsLocal)
        {
            using var input = new ProgressStream(LocalFileSystem.OpenRead(node.Source.FullPath), context);
            WriteTarget(context, destination, node.Target, () => destination.Upload(input, node.Target));
        }
        else if (destination.IsLocal)
        {
            WriteTarget(context, destination, node.Target, () =>
            {
                using var output = new ProgressStream(LocalFileSystem.OpenWrite(node.Target), context);
                source.Download(node.Source.FullPath, output);
            });
        }
        else
        {
            // Server to server: there is no direct route, so the file passes through this computer.
            using var temp = new TempFile();
            using (var output = new ProgressStream(LocalFileSystem.OpenWrite(temp.Path), context))
            {
                source.Download(node.Source.FullPath, output);
            }
            using FileStream input = LocalFileSystem.OpenRead(temp.Path);
            WriteTarget(context, destination, node.Target, () => destination.Upload(input, node.Target));
        }

        KeepTimestamp(destination, node.Target, node.Source.Modified);
        context.EndItem();
        return true;
    }

    /// <summary>Runs <paramref name="write"/> and removes the half-written target if it fails or is cancelled.</summary>
    private static void WriteTarget(OperationContext context, IFileSystem destination, string target, Action write)
    {
        try
        {
            write();
        }
        catch (Exception ex)
        {
            try
            {
                destination.DeleteFile(target);
            }
            catch (Exception cleanup) when (FileSystemErrors.IsExpected(cleanup))
            {
                // Could not clean up; the original error is the one worth reporting.
            }

            // Libraries wrap the cancellation in their own exception types.
            if (context.Token.IsCancellationRequested && ex is not OperationCanceledException)
            {
                throw new OperationCanceledException(context.Token);
            }
            throw;
        }
    }

    private static void KeepTimestamp(IFileSystem destination, string target, DateTime modified)
    {
        if (!destination.IsLocal || modified.Year < 1980)
        {
            return;
        }

        try
        {
            File.SetLastWriteTime(target, modified);
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            // The copy itself succeeded; a wrong date is not worth failing for.
        }
    }

    /// <summary>After a move by copying: removes the source folders that are empty now, deepest first.</summary>
    private static void RemoveEmptiedFolders(IFileSystem source, List<PlanNode> plan)
    {
        for (int i = plan.Count - 1; i >= 0; i--)
        {
            FileEntry folder = plan[i].Source;
            if (!folder.IsDirectory)
            {
                continue;
            }

            try
            {
                if (source.List(folder.FullPath).Count == 0)
                {
                    source.DeleteDirectory(folder.FullPath);
                }
            }
            catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
            {
                // Something was skipped or is in use: the folder simply stays.
            }
        }
    }

    #endregion

    #region Delete

    public static void Delete(OperationContext context, IFileSystem fileSystem, IReadOnlyList<FileEntry> items)
    {
        context.SetTotals(items.Count, 0);
        foreach (FileEntry item in items)
        {
            context.BeginItem(item.Name, 0);
            if (item.IsDirectory)
            {
                fileSystem.DeleteDirectory(item.FullPath);
            }
            else
            {
                fileSystem.DeleteFile(item.FullPath);
            }
            context.EndItem();
        }
    }

    #endregion

    #region Compress / extract

    /// <summary>Packs <paramref name="items"/> (files and whole folders) into a new zip archive.</summary>
    public static void Compress(
        OperationContext context,
        IFileSystem source,
        IReadOnlyList<FileEntry> items,
        IFileSystem destination,
        string archivePath)
    {
        context.Status(Strings.StatusScanning);
        var plan = new List<PlanNode>();
        foreach (FileEntry item in items)
        {
            Scan(context, source, item, item.Name, (folder, name) => folder + "/" + name, plan);
        }

        if (source.IsSameStore(destination)
            && plan.Any(node => string.Equals(node.Source.FullPath, archivePath, source.PathComparison)))
        {
            throw new IOException(Strings.SameSourceAndTarget(Path.GetFileName(archivePath)));
        }
        if (destination.FileExists(archivePath) && !context.ConfirmOverwrite(destination.GetDisplayPath(archivePath)))
        {
            return;
        }
        SetTotals(context, plan);

        // A remote archive is built here first and uploaded when complete.
        using TempFile? temp = destination.IsLocal ? null : new TempFile();
        string localArchive = temp?.Path ?? archivePath;
        try
        {
            using (var zip = new ZipArchive(LocalFileSystem.OpenWrite(localArchive), ZipArchiveMode.Create))
            {
                foreach (PlanNode node in plan)
                {
                    AddToArchive(context, source, node, zip);
                }
            }

            if (temp is not null)
            {
                context.SetTotals(1, new FileInfo(localArchive).Length);
                context.BeginItem(Path.GetFileName(archivePath), new FileInfo(localArchive).Length);
                using var input = new ProgressStream(LocalFileSystem.OpenRead(localArchive), context);
                WriteTarget(context, destination, archivePath, () => destination.Upload(input, archivePath));
                context.EndItem();
            }
        }
        catch when (temp is null)
        {
            TryDeleteLocal(localArchive); // do not leave a broken archive behind
            throw;
        }
    }

    private static void AddToArchive(OperationContext context, IFileSystem source, PlanNode node, ZipArchive zip)
    {
        if (node.Source.IsDirectory)
        {
            context.Token.ThrowIfCancellationRequested();
            zip.CreateEntry(node.Target + "/"); // keeps empty folders
            return;
        }

        context.BeginItem(node.Source.Name, node.Source.Size);
        ZipArchiveEntry entry = zip.CreateEntry(node.Target, CompressionLevel.Optimal);
        if (node.Source.Modified.Year is >= 1980 and <= 2107) // the range a zip entry can store
        {
            entry.LastWriteTime = node.Source.Modified;
        }

        using Stream output = entry.Open();
        if (source.IsLocal)
        {
            using var input = new ProgressStream(LocalFileSystem.OpenRead(node.Source.FullPath), context);
            input.CopyTo(output, BufferSize);
        }
        else
        {
            using var temp = new TempFile();
            using (var download = new ProgressStream(LocalFileSystem.OpenWrite(temp.Path), context))
            {
                source.Download(node.Source.FullPath, download);
            }
            using FileStream input = LocalFileSystem.OpenRead(temp.Path);
            input.CopyTo(output, BufferSize);
        }
        context.EndItem();
    }

    private static void TryDeleteLocal(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            // Nothing more can be done about it here.
        }
    }

    public static bool IsArchive(FileEntry item) =>
        !item.IsDirectory && Path.GetExtension(item.Name).ToLowerInvariant() is ".zip" or ".gz";

    /// <summary>Unpacks zip and gz archives into <paramref name="destinationFolder"/>.</summary>
    public static void Extract(
        OperationContext context,
        IFileSystem source,
        IReadOnlyList<FileEntry> archives,
        IFileSystem destination,
        string destinationFolder)
    {
        if (archives.FirstOrDefault(item => !IsArchive(item)) is { } unsupported)
        {
            throw new NotSupportedException(Strings.UnsupportedArchive(unsupported.Name));
        }

        destination.CreateDirectory(destinationFolder);
        foreach (FileEntry archive in archives)
        {
            using TempFile? temp = source.IsLocal ? null : new TempFile();
            if (temp is not null)
            {
                context.SetTotals(1, archive.Size);
                context.BeginItem(archive.Name, archive.Size);
                using (var download = new ProgressStream(LocalFileSystem.OpenWrite(temp.Path), context))
                {
                    source.Download(archive.FullPath, download);
                }
                context.EndItem();
            }

            string localArchive = temp?.Path ?? archive.FullPath;
            if (Path.GetExtension(archive.Name).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ExtractZip(context, localArchive, destination, destinationFolder);
            }
            else
            {
                ExtractGZip(context, localArchive, archive, destination, destinationFolder);
            }
        }
    }

    private static void ExtractZip(OperationContext context, string localArchive, IFileSystem destination, string destinationFolder)
    {
        using ZipArchive zip = ZipFile.OpenRead(localArchive);
        context.SetTotals(zip.Entries.Count(entry => !IsFolderEntry(entry)), zip.Entries.Sum(entry => entry.Length));

        var createdFolders = new HashSet<string>(StringComparer.Ordinal) { destinationFolder };
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            string[] segments = entry.FullName.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                continue;
            }
            // An archive must not be able to write outside the folder it is extracted to.
            if (segments.Any(segment => segment is "." or ".." || segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            {
                throw new InvalidDataException(Strings.UnsafeArchiveEntry(entry.FullName));
            }

            int folderDepth = IsFolderEntry(entry) ? segments.Length : segments.Length - 1;
            string folder = destinationFolder;
            for (int i = 0; i < folderDepth; i++)
            {
                folder = destination.Combine(folder, segments[i]);
                if (createdFolders.Add(folder))
                {
                    context.Token.ThrowIfCancellationRequested();
                    destination.CreateDirectory(folder);
                }
            }
            if (IsFolderEntry(entry))
            {
                continue;
            }

            string target = destination.Combine(folder, segments[^1]);
            context.BeginItem(entry.FullName, entry.Length);
            if (!destination.FileExists(target) || context.ConfirmOverwrite(destination.GetDisplayPath(target)))
            {
                using Stream content = entry.Open();
                WriteFromStream(context, content, destination, target, countBytes: true);
                KeepTimestamp(destination, target, entry.LastWriteTime.LocalDateTime);
            }
            context.EndItem();
        }
    }

    private static bool IsFolderEntry(ZipArchiveEntry entry) => entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\');

    private static void ExtractGZip(OperationContext context, string localArchive, FileEntry archive, IFileSystem destination, string destinationFolder)
    {
        // A gz file holds a single file; its size is only known after unpacking, so progress follows the input.
        string target = destination.Combine(destinationFolder, Path.GetFileNameWithoutExtension(archive.Name));
        context.SetTotals(1, archive.Size);
        context.BeginItem(archive.Name, archive.Size);
        if (!destination.FileExists(target) || context.ConfirmOverwrite(destination.GetDisplayPath(target)))
        {
            using var compressed = new ProgressStream(LocalFileSystem.OpenRead(localArchive), context);
            using var content = new GZipStream(compressed, CompressionMode.Decompress);
            WriteFromStream(context, content, destination, target, countBytes: false);
        }
        context.EndItem();
    }

    /// <summary>
    /// Writes a forward-only stream (an archive entry) to <paramref name="target"/>. FTP uploads
    /// want a real file to read from, so those are staged in a temporary file first.
    /// </summary>
    /// <param name="countBytes">False when the source stream already reports the progress.</param>
    private static void WriteFromStream(OperationContext context, Stream content, IFileSystem destination, string target, bool countBytes)
    {
        if (destination.IsLocal)
        {
            WriteTarget(context, destination, target, () =>
            {
                using FileStream file = LocalFileSystem.OpenWrite(target);
                using Stream output = countBytes ? new ProgressStream(file, context) : file;
                content.CopyTo(output, BufferSize);
            });
            return;
        }

        using var temp = new TempFile();
        using (FileStream file = LocalFileSystem.OpenWrite(temp.Path))
        using (Stream output = countBytes ? new ProgressStream(file, context) : file)
        {
            content.CopyTo(output, BufferSize);
        }
        using FileStream input = LocalFileSystem.OpenRead(temp.Path);
        WriteTarget(context, destination, target, () => destination.Upload(input, target));
    }

    #endregion

    #region Search

    /// <summary>
    /// Finds everything below <paramref name="root"/> whose name matches <paramref name="pattern"/>
    /// (* and ? wildcards; plain text matches anywhere in the name). Cancelling returns what was found so far.
    /// </summary>
    public static List<FileEntry> Search(OperationContext context, IFileSystem fileSystem, string root, string pattern)
    {
        if (pattern.IndexOfAny(['*', '?']) < 0)
        {
            pattern = "*" + pattern + "*";
        }

        var results = new List<FileEntry>();
        var pending = new Queue<string>([root]);
        while (pending.TryDequeue(out string? folder) && !context.Token.IsCancellationRequested)
        {
            try
            {
                context.Status(fileSystem.GetDisplayPath(folder));
            }
            catch (OperationCanceledException)
            {
                break; // keep what was found so far
            }

            IReadOnlyList<FileEntry> entries;
            try
            {
                entries = fileSystem.List(folder);
            }
            catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
            {
                continue; // no access to this folder: search the rest
            }

            foreach (FileEntry entry in entries)
            {
                if (FileSystemName.MatchesSimpleExpression(pattern, entry.Name, ignoreCase: true))
                {
                    results.Add(entry);
                }
                if (entry.IsDirectory && !entry.IsLink)
                {
                    pending.Enqueue(entry.FullPath);
                }
            }
        }
        return results;
    }

    #endregion
}
