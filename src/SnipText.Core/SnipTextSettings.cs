namespace SnipText.Core;

public sealed record SnipTextSettings
{
    public const string DefaultLocalAiEndpoint = "http://127.0.0.1:11434/v1/chat/completions";
    public const string DefaultLocalAiModel = "minicpm-v";
    public const double DefaultNativeLowConfidenceThreshold = 0.55d;

    public static SnipTextSettings Default { get; } = new();

    public GlobalHotkey Hotkey { get; init; } = GlobalHotkeySettings.Default.Hotkey;

    public string? OcrLanguageTag { get; init; }

    public SnipTextOutputMode OutputMode { get; init; } = SnipTextOutputMode.AutoCopy;

    public bool EnableLocalAi { get; init; }

    public LocalAiRoutingMode LocalAiRoutingMode { get; init; } = LocalAiRoutingMode.AiFallbackWhenNativeConfidenceLow;

    public string LocalAiEndpoint { get; init; } = DefaultLocalAiEndpoint;

    public string LocalAiModel { get; init; } = DefaultLocalAiModel;

    public double NativeLowConfidenceThreshold { get; init; } = DefaultNativeLowConfidenceThreshold;

    public static SnipTextSettings Normalize(SnipTextSettings? settings)
    {
        if (settings is null)
        {
            return Default;
        }

        var normalizedHotkey = settings.Hotkey.HasModifier
            ? settings.Hotkey
            : Default.Hotkey;

        var normalizedLanguageTag = string.IsNullOrWhiteSpace(settings.OcrLanguageTag)
            ? null
            : settings.OcrLanguageTag.Trim();

        var normalizedOutputMode = Enum.IsDefined(settings.OutputMode)
            ? settings.OutputMode
            : Default.OutputMode;

        var normalizedLocalAiMode = Enum.IsDefined(settings.LocalAiRoutingMode)
            ? settings.LocalAiRoutingMode
            : Default.LocalAiRoutingMode;

        var normalizedEndpoint = string.IsNullOrWhiteSpace(settings.LocalAiEndpoint)
            ? Default.LocalAiEndpoint
            : settings.LocalAiEndpoint.Trim();

        var normalizedModel = string.IsNullOrWhiteSpace(settings.LocalAiModel)
            ? Default.LocalAiModel
            : settings.LocalAiModel.Trim();

        var normalizedThreshold = double.IsNaN(settings.NativeLowConfidenceThreshold)
            ? Default.NativeLowConfidenceThreshold
            : Math.Clamp(settings.NativeLowConfidenceThreshold, 0d, 1d);

        return settings with
        {
            Hotkey = normalizedHotkey,
            OcrLanguageTag = normalizedLanguageTag,
            OutputMode = normalizedOutputMode,
            LocalAiRoutingMode = normalizedLocalAiMode,
            LocalAiEndpoint = normalizedEndpoint,
            LocalAiModel = normalizedModel,
            NativeLowConfidenceThreshold = normalizedThreshold,
        };
    }
}
