using System.Net.Security;
using FluentFTP;
using FluentFTP.Client.BaseClient;

namespace sharpcommander.FileSystems;

/// <summary>How the connection to an FTP server is protected.</summary>
internal enum FtpSecurity
{
    /// <summary>Use TLS when the server offers it, plain FTP otherwise.</summary>
    Auto,

    /// <summary>Plain, unencrypted FTP.</summary>
    None,

    /// <summary>FTPS: TLS requested on the normal FTP port.</summary>
    Explicit,

    /// <summary>FTPS: TLS from the first byte (usually port 990).</summary>
    Implicit,
}

/// <summary>A saved FTP connection. The password is deliberately not part of it.</summary>
internal sealed record FtpConnectionInfo(string Host, int Port, string User, FtpSecurity Security)
{
    public const int DefaultPort = 21;

    public override string ToString() => $"{User}@{Host}" + (Port == DefaultPort ? "" : $":{Port}");

    public bool IsSameServer(FtpConnectionInfo other) =>
        Port == other.Port
        && string.Equals(Host, other.Host, StringComparison.OrdinalIgnoreCase)
        && string.Equals(User, other.User, StringComparison.Ordinal);
}

/// <summary>An FTP / FTPS server. One instance owns one connection.</summary>
internal sealed class FtpFileSystem : IFileSystem
{
    private const int TimeoutMilliseconds = 15_000;

    // An FTP control connection handles one command at a time.
    private readonly Lock gate = new();
    private readonly FtpClient client;
    private readonly Func<string, bool> confirmUntrustedCertificate;
    private string? acceptedCertificate;

    /// <param name="confirmUntrustedCertificate">
    /// Asked (with a description of the problem) when the server's TLS certificate cannot be
    /// verified; return true to trust it for the lifetime of this connection.
    /// </param>
    public FtpFileSystem(FtpConnectionInfo connection, string password, Func<string, bool> confirmUntrustedCertificate)
    {
        Connection = connection;
        this.confirmUntrustedCertificate = confirmUntrustedCertificate;

        var config = new FtpConfig
        {
            EncryptionMode = connection.Security switch
            {
                FtpSecurity.None => FtpEncryptionMode.None,
                FtpSecurity.Explicit => FtpEncryptionMode.Explicit,
                FtpSecurity.Implicit => FtpEncryptionMode.Implicit,
                _ => FtpEncryptionMode.Auto,
            },
            ConnectTimeout = TimeoutMilliseconds,
            ReadTimeout = TimeoutMilliseconds,
            DataConnectionConnectTimeout = TimeoutMilliseconds,
            DataConnectionReadTimeout = TimeoutMilliseconds,
        };
        client = new FtpClient(connection.Host, connection.User, password, connection.Port, config);
        client.ValidateCertificate += OnValidateCertificate;
    }

    public FtpConnectionInfo Connection { get; }

    /// <summary>The folder the server put us in after login.</summary>
    public string HomePath { get; private set; } = "/";

    public void Connect()
    {
        lock (gate)
        {
            client.Connect();
            HomePath = client.GetWorkingDirectory() is { Length: > 0 } home ? home : "/";
        }
    }

    private void OnValidateCertificate(BaseFtpClient control, FtpSslValidationEventArgs e)
    {
        if (e.PolicyErrors == SslPolicyErrors.None)
        {
            e.Accept = true;
            return;
        }

        // Every data connection validates again: ask once, then only accept that same certificate.
        string certificate = e.Certificate?.GetCertHashString() ?? "";
        if (acceptedCertificate is null && confirmUntrustedCertificate($"{e.Certificate?.Subject} ({e.PolicyErrors})"))
        {
            acceptedCertificate = certificate;
        }
        e.Accept = acceptedCertificate is not null && acceptedCertificate == certificate;
    }

    public bool IsLocal => false;

    public StringComparison PathComparison => StringComparison.Ordinal;

    public string GetDisplayPath(string path) => $"ftp://{Connection}{path}";

    public bool IsSameStore(IFileSystem other) =>
        other is FtpFileSystem ftp && ftp.Connection.IsSameServer(Connection);

    public IReadOnlyList<FileEntry> List(string path)
    {
        FtpListItem[] items;
        lock (gate)
        {
            items = client.GetListing(path);
        }

        return
        [
            .. items
                .Where(item => item.Name is not ("." or ".."))
                .Select(item => new FileEntry(item.Name, item.FullName, IsDirectory(item), Math.Max(item.Size, 0), item.Modified, item.Type == FtpObjectType.Link))
                .OrderByDescending(entry => entry.IsDirectory)
                .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase),
        ];
    }

    private static bool IsDirectory(FtpListItem item) =>
        item.Type == FtpObjectType.Directory
        || (item.Type == FtpObjectType.Link && item.LinkObject?.Type == FtpObjectType.Directory);

    public string? GetParent(string path)
    {
        string trimmed = path.TrimEnd('/');
        if (trimmed.Length == 0)
        {
            return null;
        }
        int slash = trimmed.LastIndexOf('/');
        return slash <= 0 ? "/" : trimmed[..slash];
    }

    public string Combine(string path, string name) => path.TrimEnd('/') + "/" + name;

    public bool FileExists(string path)
    {
        lock (gate)
        {
            return client.FileExists(path);
        }
    }

    public bool DirectoryExists(string path)
    {
        lock (gate)
        {
            return client.DirectoryExists(path);
        }
    }

    public void CreateDirectory(string path)
    {
        lock (gate)
        {
            client.CreateDirectory(path, true);
        }
    }

    public void DeleteFile(string path)
    {
        lock (gate)
        {
            client.DeleteFile(path);
        }
    }

    public void DeleteDirectory(string path)
    {
        lock (gate)
        {
            client.DeleteDirectory(path);
        }
    }

    public void Move(string source, string destination)
    {
        lock (gate)
        {
            client.Rename(source, destination);
        }
    }

    public void Download(string path, Stream target)
    {
        lock (gate)
        {
            if (!client.DownloadStream(target, path))
            {
                throw new IOException(GetDisplayPath(path));
            }
        }
    }

    public void Upload(Stream source, string path)
    {
        lock (gate)
        {
            if (client.UploadStream(source, path, FtpRemoteExists.Overwrite, true) == FtpStatus.Failed)
            {
                throw new IOException(GetDisplayPath(path));
            }
        }
    }

    public void Dispose()
    {
        client.ValidateCertificate -= OnValidateCertificate;
        try
        {
            client.Dispose();
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            // The connection may already be gone; there is nothing left to clean up.
        }
    }
}
