using sharpcommander.FileSystems;
using sharpcommander.Localization;

namespace sharpcommander;

/// <summary>Asks for the details of an FTP connection, optionally starting from a saved one.</summary>
public partial class FtpConnectDialog : Form
{
    private const string AnonymousUser = "anonymous";

    private sealed record SecurityChoice(FtpSecurity Value, string Text)
    {
        public override string ToString() => Text;
    }

    /// <param name="startBlank">True to enter a new connection; false to start from the most recently used one.</param>
    public FtpConnectDialog(bool startBlank)
    {
        InitializeComponent();
        Localizer.Localize(this);

        securityCombo.Items.AddRange(
        [
            new SecurityChoice(FtpSecurity.Auto, Strings.FtpSecurityAuto),
            new SecurityChoice(FtpSecurity.None, Strings.FtpSecurityNone),
            new SecurityChoice(FtpSecurity.Explicit, Strings.FtpSecurityExplicit),
            new SecurityChoice(FtpSecurity.Implicit, Strings.FtpSecurityImplicit),
        ]);
        securityCombo.SelectedIndex = 0;

        savedCombo.Items.AddRange([.. AppSettings.Current.FtpConnections]);
        savedCombo.Enabled = savedCombo.Items.Count > 0;
        if (!startBlank && savedCombo.Items.Count > 0)
        {
            savedCombo.SelectedIndex = 0; // fills the fields
        }
    }

    /// <summary>The connection that was entered. Valid after the dialog returned OK.</summary>
    internal FtpConnectionInfo Connection { get; private set; } =
        new("", FtpConnectionInfo.DefaultPort, AnonymousUser, FtpSecurity.Auto);

    internal string Password => passwordBox.Text;

    internal bool SaveConnection => saveCheck.Checked;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        (hostBox.TextLength == 0 ? hostBox : passwordBox).Select();
    }

    private void savedCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (savedCombo.SelectedItem is not FtpConnectionInfo saved)
        {
            return;
        }

        hostBox.Text = saved.Host;
        portBox.Value = Math.Clamp(saved.Port, portBox.Minimum, portBox.Maximum);
        userBox.Text = saved.User;
        passwordBox.Clear();
        securityCombo.SelectedItem = securityCombo.Items.Cast<SecurityChoice>().First(choice => choice.Value == saved.Security);
    }

    private void connectBtn_Click(object? sender, EventArgs e)
    {
        // People paste addresses: accept "ftp://host/" as well as a bare host name.
        string host = hostBox.Text.Trim();
        if (host.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
        {
            host = host["ftp://".Length..];
        }
        host = host.TrimEnd('/');

        if (host.Length == 0)
        {
            MessageBox.Show(this, Strings.FtpHostRequired, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            hostBox.Select();
            return;
        }

        string user = userBox.Text.Trim();
        Connection = new FtpConnectionInfo(
            host,
            (int)portBox.Value,
            user.Length == 0 ? AnonymousUser : user,
            ((SecurityChoice)securityCombo.SelectedItem!).Value);
        DialogResult = DialogResult.OK;
    }
}
