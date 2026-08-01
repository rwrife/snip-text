namespace SnipText.Core;

public sealed record GlobalHotkey(HotkeyModifiers Modifiers, int VirtualKey)
{
    public bool HasModifier => (Modifiers & ~HotkeyModifiers.NoRepeat) != HotkeyModifiers.None;

    public string DisplayText
    {
        get
        {
            var parts = new List<string>();

            if (Modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
            if (Modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
            if (Modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
            if (Modifiers.HasFlag(HotkeyModifiers.Win)) parts.Add("Win");

            parts.Add(VirtualKeyToText(VirtualKey));
            return string.Join("+", parts);
        }
    }

    private static string VirtualKeyToText(int virtualKey)
    {
        if (virtualKey is >= 0x30 and <= 0x39)
        {
            return ((char)virtualKey).ToString();
        }

        if (virtualKey is >= 0x41 and <= 0x5A)
        {
            return ((char)virtualKey).ToString();
        }

        return $"VK_{virtualKey:X2}";
    }
}
