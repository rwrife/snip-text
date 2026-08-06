using System.Net.Http;
using System.Windows;
using SnipText.Capture;
using SnipText.Core;
using SnipText.Infrastructure;
using SnipText.Preview;
using SnipText.Recognition;
using SnipText.Settings;

namespace SnipText;

public partial class App : System.Windows.Application
{
    private readonly CaptureOverlayService _captureOverlayService = new();
    private readonly ScreenRegionCaptureService _screenRegionCaptureService =
        new(new SystemDrawingScreenRegionCaptureBackend());
    private readonly ISnipTextSettingsStore _settingsStore = new JsonSnipTextSettingsStore();
    private readonly HttpClient _httpClient = new();

    private ITextRecognizer _textRecognizer = new WindowsOcrRecognizer();
    private SnipTextSettings _settings = SnipTextSettings.Default;

    private TrayIconHost? _trayIcon;
    private GlobalHotkeyManager? _hotkeyManager;

    public event EventHandler? CaptureRequested;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _settings = SnipTextSettings.Normalize(await _settingsStore.LoadAsync());
        _textRecognizer = BuildTextRecognizer(_settings);

        _trayIcon = new TrayIconHost();
        _trayIcon.CaptureClicked += (_, _) => RaiseCaptureRequested();
        _trayIcon.SettingsClicked += async (_, _) => await ShowSettingsWindowAsync();
        _trayIcon.ExitClicked += (_, _) => Shutdown();

        _hotkeyManager = new GlobalHotkeyManager();
        _hotkeyManager.CaptureRequested += (_, _) => RaiseCaptureRequested();

        RegisterHotkey(_settings.Hotkey);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyManager?.Dispose();
        _trayIcon?.Dispose();
        _httpClient.Dispose();
        base.OnExit(e);
    }

    private async Task ShowSettingsWindowAsync()
    {
        var window = new SettingsWindow(_settings);

        if (window.ShowDialog() != true || window.SavedSettings is null)
        {
            return;
        }

        _settings = window.SavedSettings;

        try
        {
            await _settingsStore.SaveAsync(_settings);
            _textRecognizer = BuildTextRecognizer(_settings);
            RegisterHotkey(_settings.Hotkey);
            _trayIcon?.ShowInfo("snip-text", "Settings saved.");
        }
        catch (Exception ex)
        {
            _trayIcon?.ShowWarning("snip-text settings", $"Failed to save settings: {ex.Message}");
        }
    }

    private ITextRecognizer BuildTextRecognizer(SnipTextSettings settings)
    {
        var nativeRecognizer = new WindowsOcrRecognizer(settings.OcrLanguageTag);

        if (!settings.EnableLocalAi)
        {
            return nativeRecognizer;
        }

        try
        {
            var localAiRecognizer = new LocalAiVisionRecognizer(
                _httpClient,
                settings.LocalAiEndpoint,
                settings.LocalAiModel);

            return new AdaptiveTextRecognizer(
                nativeRecognizer,
                localAiRecognizer,
                settings.LocalAiRoutingMode,
                settings.NativeLowConfidenceThreshold);
        }
        catch (ArgumentException ex)
        {
            _trayIcon?.ShowWarning("snip-text local-AI settings", $"Local-AI disabled: {ex.Message}");
            return nativeRecognizer;
        }
    }

    private void RegisterHotkey(GlobalHotkey hotkey)
    {
        if (_hotkeyManager is null)
        {
            return;
        }

        var registration = _hotkeyManager.TryRegister(hotkey);
        if (!registration.Success)
        {
            _trayIcon?.ShowWarning(
                "snip-text hotkey unavailable",
                $"Could not register {hotkey.DisplayText}. It may already be in use.");
        }
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

            if (_settings.OutputMode == SnipTextOutputMode.Preview)
            {
                var previewWindow = new EditablePreviewWindow(recognizedText);
                previewWindow.Show();
                previewWindow.Activate();
                _trayIcon?.ShowInfo("snip-text", "Preview opened. Edit and click Copy to update the clipboard.");
                return;
            }

            Clipboard.SetText(recognizedText);
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
