using sharpcommander.FileSystems;
using sharpcommander.Localization;
using sharpcommander.Operations;

namespace sharpcommander;

/// <summary>
/// Runs a file operation on a background thread while showing its progress, with a Cancel button.
/// Use <see cref="Run"/>.
/// </summary>
public partial class ProcessForm : Form
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly Action<OperationContext> work;
    private Task? task;
    private bool completed;

    private ProcessForm(string title, Action<OperationContext> work)
    {
        InitializeComponent();
        Localizer.Localize(this);
        this.work = work;
        Text = title;

        // The designer left a spare button and labels sized for placeholder text.
        button1.Visible = false;
        AcceptButton = null;
        feladatLbl.AutoSize = false;
        feladatLbl.AutoEllipsis = true;
        feladatLbl.Width = dbLbl.Left - feladatLbl.Left - 8;
        foreach (Label counter in new[] { dbLbl, overdbLbl })
        {
            counter.AutoSize = false;
            counter.TextAlign = ContentAlignment.TopRight;
            counter.SetBounds(taskProg.Right - 120, counter.Top, 120, counter.Height);
        }
        feladatLbl.Width = dbLbl.Left - feladatLbl.Left - 8;
        feladatLbl.Text = dbLbl.Text = overdbLbl.Text = "";
        StartPosition = FormStartPosition.CenterParent;
    }

    /// <summary>
    /// Runs <paramref name="work"/> and returns when it has ended. Errors are shown to the user.
    /// </summary>
    /// <returns>True if the operation ran to the end; false if it failed or was cancelled.</returns>
    internal static bool Run(IWin32Window owner, string title, Action<OperationContext> work)
    {
        using var form = new ProcessForm(title, work);
        form.ShowDialog(owner);
        return form.completed;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        var context = new OperationContext(cancellation.Token, ReportProgress, AskOverwrite);
        task = Task.Run(() => work(context));
        try
        {
            await task;
            completed = true;
        }
        catch (OperationCanceledException)
        {
            // The user asked for it; nothing to report.
        }
        catch (Exception ex) when (FileSystemErrors.IsExpected(ex))
        {
            if (!cancellation.IsCancellationRequested)
            {
                MessageBox.Show(this, FileSystemErrors.Describe(ex), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Cancel, Esc and the close box only request the stop; the form closes once the worker has ended.
        if (task is { IsCompleted: false })
        {
            e.Cancel = true;
            cancelBtn.Enabled = false;
            cancellation.Cancel();
        }
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        cancellation.Dispose();
    }

    private void ReportProgress(OperationProgress progress)
    {
        // Called on the worker thread.
        if (IsHandleCreated && !IsDisposed)
        {
            BeginInvoke(() => ShowProgress(progress));
        }
    }

    private void ShowProgress(OperationProgress progress)
    {
        feladatLbl.Text = progress.Item;
        SetBar(taskProg, progress.ItemDone, progress.ItemTotal);

        dbLbl.Text = progress.FilesTotal > 0 ? $"{Math.Min(progress.FilesDone + 1, progress.FilesTotal)} / {progress.FilesTotal}" : "";
        if (progress.BytesTotal > 0)
        {
            SetBar(overProg, progress.BytesDone, progress.BytesTotal);
        }
        else
        {
            SetBar(overProg, progress.FilesDone, progress.FilesTotal);
        }
        overdbLbl.Text = overProg.Style == ProgressBarStyle.Blocks ? $"{overProg.Value / 10} %" : "";
    }

    private static void SetBar(ProgressBar bar, long done, long total)
    {
        if (total <= 0)
        {
            bar.Style = ProgressBarStyle.Marquee; // length unknown
            return;
        }

        bar.Style = ProgressBarStyle.Blocks;
        bar.Maximum = 1000;
        bar.Value = (int)Math.Clamp(done * 1000 / total, 0, 1000);
    }

    private OverwriteAnswer AskOverwrite(string displayPath)
    {
        // Called on the worker thread, which waits here for the answer.
        return Invoke(() =>
        {
            using var dialog = new AreYouSureForm(Strings.ConfirmOverwriteItem(displayPath), offerAll: true);
            return dialog.ShowDialog(this) switch
            {
                DialogResult.OK => OverwriteAnswer.Yes,
                DialogResult.Yes => OverwriteAnswer.All,
                _ => OverwriteAnswer.No,
            };
        });
    }
}
