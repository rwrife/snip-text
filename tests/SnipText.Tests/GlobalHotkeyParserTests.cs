using SnipText.Core;
using Xunit;

namespace SnipText.Tests;

public sealed class GlobalHotkeyParserTests
{
    [Fact]
    public void TryParse_ParsesAlphaHotkey()
    {
        var success = GlobalHotkeyParser.TryParse("Ctrl+Shift+O", out var hotkey, out var error);

        Assert.True(success);
        Assert.Null(error);
        Assert.Equal(HotkeyModifiers.Control | HotkeyModifiers.Shift, hotkey.Modifiers);
        Assert.Equal(0x4F, hotkey.VirtualKey);
    }

    [Fact]
    public void TryParse_ParsesFunctionKey()
    {
        var success = GlobalHotkeyParser.TryParse("Alt+F12", out var hotkey, out _);

        Assert.True(success);
        Assert.Equal(HotkeyModifiers.Alt, hotkey.Modifiers);
        Assert.Equal(0x7B, hotkey.VirtualKey);
    }

    [Fact]
    public void TryParse_FailsWithoutModifier()
    {
        var success = GlobalHotkeyParser.TryParse("O", out _, out var error);

        Assert.False(success);
        Assert.Contains("modifier", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryParse_FailsForUnknownModifier()
    {
        var success = GlobalHotkeyParser.TryParse("Caps+O", out _, out var error);

        Assert.False(success);
        Assert.Contains("Unsupported modifier", error);
    }
}
