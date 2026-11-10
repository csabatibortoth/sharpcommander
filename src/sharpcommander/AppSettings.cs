using System.Text.Json;
using System.Text.Json.Serialization;
using sharpcommander.FileSystems;

namespace sharpcommander;

/// <summary>
/// The user's preferences, kept in <c>%AppData%\Sharp Commander\settings.json</c>.
/// Passwords are never stored.
/// </summary>
internal sealed class AppSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sharp Commander", "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private static AppSettings? current;

    /// <summary>The settings of this user; loaded on first use.</summary>
    public static AppSettings Current => current ??= Load();

    /// <summary>UI language (a culture name), or <c>null</c> to follow the system language.</summary>
    public string? Language { get; set; }

    /// <summary>Saved FTP connections, most recently used first.</summary>
    public List<FtpConnectionInfo> FtpConnections { get; set; } = [];

    /// <summary>Puts <paramref name="connection"/> at the top of the saved connections.</summary>
    public void RememberFtpConnection(FtpConnectionInfo connection)
    {
        FtpConnections.RemoveAll(connection.IsSameServer);
        FtpConnections.Insert(0, connection);
        Save();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Not being able to remember a preference must not break what the user is doing.
        }
    }

    private static AppSettings Load()
    {
        try
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), JsonOptions) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new AppSettings(); // no (readable) settings yet
        }
    }
}
