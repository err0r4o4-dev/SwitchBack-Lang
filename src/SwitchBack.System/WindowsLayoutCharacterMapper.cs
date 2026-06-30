using System.Runtime.InteropServices;
using System.Text;
using SwitchBack.Core;

namespace SwitchBack.SystemServices;

public sealed class WindowsLayoutCharacterMapper : ICharacterLayoutMapper
{
    private const uint MapVirtualKeyToScanCode = 0;
    private const uint DoNotChangeKeyboardState = 0x04;
    private const byte KeyDown = 0x80;
    private const int VirtualKeyShift = 0x10;
    private const int VirtualKeyControl = 0x11;
    private const int VirtualKeyMenu = 0x12;

    private readonly InputLayoutInfo _source;
    private readonly InputLayoutInfo _target;

    public WindowsLayoutCharacterMapper(InputLayoutInfo source, InputLayoutInfo target)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _target = target ?? throw new ArgumentNullException(nameof(target));

        if (!source.IsSupported || !target.IsSupported)
        {
            throw new NotSupportedException("IME-based input profiles are not supported by position mapping.");
        }
    }

    public string SourceLayoutId => _source.Id;

    public string TargetLayoutId => _target.Id;

    public bool TryMap(char input, out string output)
    {
        output = string.Empty;

        var keyAndModifiers = VkKeyScanEx(input, _source.Handle);
        if (keyAndModifiers == -1)
        {
            return false;
        }

        var virtualKey = keyAndModifiers & 0xFF;
        var modifiers = (keyAndModifiers >> 8) & 0xFF;
        var keyboardState = new byte[256];

        if ((modifiers & 1) != 0) keyboardState[VirtualKeyShift] = KeyDown;
        if ((modifiers & 2) != 0) keyboardState[VirtualKeyControl] = KeyDown;
        if ((modifiers & 4) != 0) keyboardState[VirtualKeyMenu] = KeyDown;

        var scanCode = MapVirtualKeyEx((uint)virtualKey, MapVirtualKeyToScanCode, _source.Handle);
        var buffer = new StringBuilder(8);
        var characterCount = ToUnicodeEx(
            (uint)virtualKey,
            scanCode,
            keyboardState,
            buffer,
            buffer.Capacity,
            DoNotChangeKeyboardState,
            _target.Handle);

        if (characterCount <= 0)
        {
            return false;
        }

        output = buffer.ToString(0, Math.Min(characterCount, buffer.Length));
        return !string.IsNullOrEmpty(output);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern short VkKeyScanEx(char character, IntPtr keyboardLayout);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint MapVirtualKeyEx(uint code, uint mapType, IntPtr keyboardLayout);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ToUnicodeEx(
        uint virtualKey,
        uint scanCode,
        byte[] keyboardState,
        [Out] StringBuilder buffer,
        int bufferSize,
        uint flags,
        IntPtr keyboardLayout);
}
