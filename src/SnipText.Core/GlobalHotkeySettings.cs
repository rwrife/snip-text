namespace SnipText.Core;

public sealed class GlobalHotkeySettings
{
    public static GlobalHotkeySettings Default { get; } = new()
    {
        Hotkey = new GlobalHotkey(HotkeyModifiers.Control | HotkeyModifiers.Shift, 0x4F),
    };

    public required GlobalHotkey Hotkey { get; init; }
}
