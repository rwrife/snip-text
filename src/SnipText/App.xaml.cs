using System.Windows;
using SnipText.Capture;
using SnipText.Core;
using SnipText.Infrastructure;
using SnipText.Recognition;

namespace SnipText;

public partial class App : System.Windows.Application
{
    private readonly CaptureOverlayService _captureOverlayService = new();
    private readonly ScreenRegionCaptureService _screenRegionCaptureService =
        new(new SystemDrawingScreenRegionCaptureBackend());
    private readonly ITextRecognizer _textRecognizer = new WindowsOcrRecognizer();

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

    private async void RaiseCaptureRequested()
    {
        CaptureRequested?.Invoke(this, EventArgs.Empty);

        var selectedBounds = _captureOverlayService.ShowAndSelect();
        if (selectedBounds is null)
        {
            _trayIcon?.ShowInfo("snip-text", "Capture cancelled.");
            return;
        }

        try
        {
            var captured = _screenRegionCaptureService.Capture(selectedBounds.Value);
            var recognizedText = await _textRecognizer.RecognizeAsync(captured);

            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                _trayIcon?.ShowWarning("snip-text", "No text detected in the selected region.");
                return;
            }

            System.Windows.Clipboard.SetText(recognizedText);
            _trayIcon?.ShowInfo(
                "snip-text",
                $"Copied {recognizedText.Length} characters to clipboard.");
        }
        catch (InvalidOperationException ex)
        {
            _trayIcon?.ShowWarning("snip-text OCR unavailable", ex.Message);
        }
        catch (Exception ex)
        {
            _trayIcon?.ShowWarning("snip-text OCR failed", ex.Message);
        }
    }
}
