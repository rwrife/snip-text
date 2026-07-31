using System.Windows;
using SnipText.Core;
using SnipText.Infrastructure;

namespace SnipText;

public partial class App : System.Windows.Application
{
    private TrayIconHost? _trayIcon;
    private GlobalHotkeyManager? _hotkeyManager;

    public event EventHandler? CaptureRequested;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _trayIcon = new TrayIconHost();
        _trayIcon.CaptureClicked += (_, _) => RaiseCaptureRequested();
        _trayIcon.SettingsClicked += (_, _) => _trayIcon.ShowInfo("snip-text", "Settings window is not implemented yet.");
        _trayIcon.ExitClicked += (_, _) => Shutdown();

        _hotkeyManager = new GlobalHotkeyManager();
        _hotkeyManager.CaptureRequested += (_, _) => RaiseCaptureRequested();

        var registration = _hotkeyManager.TryRegister(GlobalHotkeySettings.Default.Hotkey);
        if (!registration.Success)
        {
            _trayIcon.ShowWarning(
                "snip-text hotkey unavailable",
                $"Could not register {GlobalHotkeySettings.Default.Hotkey.DisplayText}. It may already be in use.");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyManager?.Dispose();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }

    private void RaiseCaptureRequested()
    {
        CaptureRequested?.Invoke(this, EventArgs.Empty);
        _trayIcon?.ShowInfo("snip-text", "Capture requested.");
    }
}
