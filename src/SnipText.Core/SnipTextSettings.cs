namespace SnipText.Core;

public sealed record SnipTextSettings
{
    public static SnipTextSettings Default { get; } = new();

    public GlobalHotkey Hotkey { get; init; } = GlobalHotkeySettings.Default.Hotkey;

    public string? OcrLanguageTag { get; init; }

    public SnipTextOutputMode OutputMode { get; init; } = SnipTextOutputMode.AutoCopy;

    public bool EnableLocalAi { get; init; }

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

        return settings with
        {
            Hotkey = normalizedHotkey,
            OcrLanguageTag = normalizedLanguageTag,
            OutputMode = normalizedOutputMode,
        };
    }
}
