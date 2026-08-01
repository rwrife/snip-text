using System.Runtime.InteropServices;
using System.Windows.Interop;
using SnipText.Core;

namespace SnipText.Infrastructure;

public sealed class GlobalHotkeyManager : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotkeyId = 0x534E4950;

    private readonly HwndSource _source;
    private bool _isRegistered;
    private bool _disposed;

    public event EventHandler? CaptureRequested;

    public GlobalHotkeyManager()
    {
        var parameters = new HwndSourceParameters("SnipTextHotkeySink")
        {
            Width = 0,
            Height = 0,
            WindowStyle = 0,
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    public HotkeyRegistrationResult TryRegister(GlobalHotkey hotkey)
    {
        if (!hotkey.HasModifier)
        {
            return HotkeyRegistrationResult.Failed("At least one modifier is required for a global hotkey.");
        }

        if (_isRegistered)
        {
            Unregister();
        }

        var success = RegisterHotKey(_source.Handle, HotkeyId, (uint)hotkey.Modifiers, (uint)hotkey.VirtualKey);
        if (!success)
        {
            var errorCode = Marshal.GetLastWin32Error();
            return HotkeyRegistrationResult.Failed($"RegisterHotKey failed with Win32 error {errorCode}.");
        }

        _isRegistered = true;
        return HotkeyRegistrationResult.Ok();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Unregister();
        _source.RemoveHook(WndProc);
        _source.Dispose();
        _disposed = true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            CaptureRequested?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void Unregister()
    {
        if (!_isRegistered)
        {
            return;
        }

        UnregisterHotKey(_source.Handle, HotkeyId);
        _isRegistered = false;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
