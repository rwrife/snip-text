using System.Globalization;

namespace SnipText.Core;

public static class GlobalHotkeyParser
{
    public static bool TryParse(string? text, out GlobalHotkey hotkey, out string? error)
    {
        hotkey = GlobalHotkeySettings.Default.Hotkey;
        error = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            error = "Hotkey cannot be empty.";
            return false;
        }

        var tokens = text
            .Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        if (tokens.Length < 2)
        {
            error = "Hotkey must include at least one modifier and one key, for example Ctrl+Shift+O.";
            return false;
        }

        var modifiers = HotkeyModifiers.None;

        for (var i = 0; i < tokens.Length - 1; i++)
        {
            var token = tokens[i];
            if (token.Equals("ctrl", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("control", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Control;
                continue;
            }

            if (token.Equals("shift", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Shift;
                continue;
            }

            if (token.Equals("alt", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Alt;
                continue;
            }

            if (token.Equals("win", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("windows", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("meta", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Win;
                continue;
            }

            error = $"Unsupported modifier '{token}'. Supported modifiers: Ctrl, Shift, Alt, Win.";
            return false;
        }

        if (modifiers == HotkeyModifiers.None)
        {
            error = "At least one modifier is required for a global hotkey.";
            return false;
        }

        var keyToken = tokens[^1];
        if (!TryParseVirtualKey(keyToken, out var virtualKey))
        {
            error = $"Unsupported key '{keyToken}'. Use A-Z, 0-9, F1-F24, or VK_XX hex key codes.";
            return false;
        }

        hotkey = new GlobalHotkey(modifiers, virtualKey);
        return true;
    }

    private static bool TryParseVirtualKey(string keyToken, out int virtualKey)
    {
        virtualKey = 0;

        if (keyToken.Length == 1 && char.IsLetterOrDigit(keyToken[0]))
        {
            virtualKey = char.ToUpperInvariant(keyToken[0]);
            return true;
        }

        if (keyToken.StartsWith("F", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(keyToken.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var functionKeyNumber) &&
            functionKeyNumber is >= 1 and <= 24)
        {
            virtualKey = 0x70 + functionKeyNumber - 1;
            return true;
        }

        if (keyToken.StartsWith("VK_", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(keyToken.AsSpan(3), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsedHex))
        {
            virtualKey = parsedHex;
            return true;
        }

        return false;
    }
}
