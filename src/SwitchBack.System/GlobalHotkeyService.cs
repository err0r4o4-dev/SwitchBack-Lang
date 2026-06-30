using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using SwitchBack.Config;

namespace SwitchBack.SystemServices;

public sealed class GlobalHotkeyService : IDisposable
{
    public const int HotkeyMessage = 0x0312;
    private const int HotkeyId = 0x5342;

    private IntPtr _windowHandle;
    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public void Register(IntPtr windowHandle, HotkeySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Unregister();

        var modifiers = HotkeyModifiers.NoRepeat;
        if (settings.Control) modifiers |= HotkeyModifiers.Control;
        if (settings.Shift) modifiers |= HotkeyModifiers.Shift;
        if (settings.Alt) modifiers |= HotkeyModifiers.Alt;
        if (settings.Windows) modifiers |= HotkeyModifiers.Windows;

        var virtualKey = GetVirtualKey(settings.Key);
        if (!RegisterHotKey(windowHandle, HotkeyId, (uint)modifiers, (uint)virtualKey))
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                $"Hotkey {settings} is unavailable. Another application may already use it.");
        }

        _windowHandle = windowHandle;
        _registered = true;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterHotKey(_windowHandle, HotkeyId);
        _registered = false;
        _windowHandle = IntPtr.Zero;
    }

    public bool ProcessWindowMessage(int message, IntPtr wParam)
    {
        if (message != HotkeyMessage || wParam.ToInt32() != HotkeyId)
        {
            return false;
        }

        HotkeyPressed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public static int GetVirtualKey(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
        {
            throw new ArgumentException("A hotkey key is required.", nameof(keyName));
        }

        var converter = new KeyConverter();
        if (converter.ConvertFromString(keyName.Trim()) is not Key key || key == Key.None)
        {
            throw new ArgumentException($"Unsupported hotkey key: {keyName}", nameof(keyName));
        }

        return KeyInterop.VirtualKeyFromKey(key);
    }

    public void Dispose()
    {
        Unregister();
        GC.SuppressFinalize(this);
    }

    [Flags]
    private enum HotkeyModifiers : uint
    {
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Windows = 0x0008,
        NoRepeat = 0x4000
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr windowHandle, int id);
}
