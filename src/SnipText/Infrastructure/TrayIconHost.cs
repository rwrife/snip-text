using System.Drawing;
using System.Windows.Forms;

namespace SnipText.Infrastructure;

public sealed class TrayIconHost : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;

    public event EventHandler? CaptureClicked;
    public event EventHandler? SettingsClicked;
    public event EventHandler? ExitClicked;

    public TrayIconHost()
    {
        _menu = new ContextMenuStrip();
        _menu.Items.Add("Capture", null, (_, _) => CaptureClicked?.Invoke(this, EventArgs.Empty));
        _menu.Items.Add("Settings", null, (_, _) => SettingsClicked?.Invoke(this, EventArgs.Empty));
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add("Exit", null, (_, _) => ExitClicked?.Invoke(this, EventArgs.Empty));

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "snip-text",
            Visible = true,
            ContextMenuStrip = _menu,
        };

        _notifyIcon.DoubleClick += (_, _) => CaptureClicked?.Invoke(this, EventArgs.Empty);
    }

    public void ShowWarning(string title, string message)
    {
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
        _notifyIcon.ShowBalloonTip(5000);
    }

    public void ShowInfo(string title, string message)
    {
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(2500);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }
}
