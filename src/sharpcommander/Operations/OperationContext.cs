using System.Diagnostics;

namespace sharpcommander.Operations;

internal enum OverwriteAnswer
{
    Yes,
    No,
    All,
}

/// <summary>A snapshot of how far a file operation has got. Totals of 0 mean "unknown".</summary>
internal readonly record struct OperationProgress(
    string Item,
    long ItemDone,
    long ItemTotal,
    int FilesDone,
    int FilesTotal,
    long BytesDone,
    long BytesTotal);

/// <summary>
/// The link between a file operation running on a background thread and whoever started it:
/// cancellation, progress reporting and questions to the user.
/// </summary>
internal sealed class OperationContext(
    CancellationToken token,
    Action<OperationProgress> report,
    Func<string, OverwriteAnswer> askOverwrite)
{
    // Progress arrives per buffer; the screen does not need more than a few updates per second.
    private static readonly long ReportInterval = Stopwatch.Frequency / 15;

    private long lastReport;
    private bool overwriteAll;

    private string item = "";
    private long itemDone, itemTotal, bytesDone, bytesTotal;
    private int filesDone, filesTotal;

    public CancellationToken Token { get; } = token;

    /// <summary>Asks whether an existing file may be replaced; "All" is remembered for the rest of the operation.</summary>
    public bool ConfirmOverwrite(string displayPath)
    {
        if (overwriteAll)
        {
            return true;
        }

        OverwriteAnswer answer = askOverwrite(displayPath);
        overwriteAll = answer == OverwriteAnswer.All;
        return answer != OverwriteAnswer.No;
    }

    /// <summary>Shows a phase whose length is not known (scanning, deleting, searching).</summary>
    public void Status(string text)
    {
        Token.ThrowIfCancellationRequested();
        item = text;
        itemDone = itemTotal = 0;
        Publish(force: false);
    }

    public void SetTotals(int files, long bytes)
    {
        filesDone = 0;
        bytesDone = 0;
        filesTotal = files;
        bytesTotal = bytes;
    }

    public void BeginItem(string name, long size)
    {
        Token.ThrowIfCancellationRequested();
        item = name;
        itemDone = 0;
        itemTotal = size;
        Publish(force: true);
    }

    public void AddBytes(long count)
    {
        itemDone += count;
        bytesDone += count;
        Publish(force: false);
    }

    /// <summary>Marks the current item as finished (or skipped) so the totals stay in step.</summary>
    public void EndItem()
    {
        bytesDone += Math.Max(itemTotal - itemDone, 0);
        itemDone = itemTotal;
        filesDone++;
        Publish(force: true);
    }

    private void Publish(bool force)
    {
        long now = Stopwatch.GetTimestamp();
        if (!force && now - lastReport < ReportInterval)
        {
            return;
        }

        lastReport = now;
        report(new OperationProgress(item, itemDone, itemTotal, filesDone, filesTotal, bytesDone, bytesTotal));
    }
}

/// <summary>
/// Passes everything through to another stream while counting the bytes for the progress
/// display and stopping the transfer when the operation is cancelled.
/// </summary>
internal sealed class ProgressStream(Stream inner, OperationContext context, bool leaveOpen = false) : Stream
{
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => inner.CanWrite;
    public override long Length => inner.Length;

    public override long Position
    {
        get => inner.Position;
        set => inner.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        context.Token.ThrowIfCancellationRequested();
        int read = inner.Read(buffer, offset, count);
        context.AddBytes(read);
        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        context.Token.ThrowIfCancellationRequested();
        inner.Write(buffer, offset, count);
        context.AddBytes(count);
    }

    public override void Flush() => inner.Flush();

    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

    public override void SetLength(long value) => inner.SetLength(value);

    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen)
        {
            inner.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>A scratch file that is deleted again when disposed.</summary>
internal sealed class TempFile : IDisposable
{
    public static string Folder { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SharpCommander");

    public TempFile()
    {
        Directory.CreateDirectory(Folder);
        Path = System.IO.Path.Combine(Folder, Guid.NewGuid().ToString("N") + ".tmp");
    }

    public string Path { get; }

    public void Dispose()
    {
        try
        {
            File.Delete(Path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Still in use somewhere; Windows cleans the temp folder eventually.
        }
    }
}
