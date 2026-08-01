using SnipText.Core;
using Xunit;

namespace SnipText.Tests;

public class GlobalHotkeyTests
{
    [Fact]
    public void DefaultHotkey_IsCtrlShiftO()
    {
        var hotkey = GlobalHotkeySettings.Default.Hotkey;

        Assert.Equal(HotkeyModifiers.Control | HotkeyModifiers.Shift, hotkey.Modifiers);
        Assert.Equal(0x4F, hotkey.VirtualKey);
    }

    [Fact]
    public void DisplayText_FormatsModifierAndKey()
    {
        var hotkey = new GlobalHotkey(HotkeyModifiers.Control | HotkeyModifiers.Shift, 0x4F);

        Assert.Equal("Ctrl+Shift+O", hotkey.DisplayText);
    }

    [Fact]
    public void HasModifier_FalseWithoutAnyModifier()
    {
        var hotkey = new GlobalHotkey(HotkeyModifiers.None, 0x4F);

        Assert.False(hotkey.HasModifier);
    }
}
