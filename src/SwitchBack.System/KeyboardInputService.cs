using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SwitchBack.SystemServices;

public sealed class KeyboardInputService
{
    private const ushort VirtualKeyControl = 0x11;
    private const ushort VirtualKeyShift = 0x10;
    private const ushort VirtualKeyAlt = 0x12;
    private const ushort VirtualKeyLeftWindows = 0x5B;
    private const ushort VirtualKeyRightWindows = 0x5C;
    private const ushort VirtualKeyC = 0x43;
    private const ushort VirtualKeyV = 0x56;
    private const uint InputKeyboard = 1;
    private const uint KeyEventKeyUp = 0x0002;

    internal static int NativeInputSize => Marshal.SizeOf<Input>();

    public async Task WaitForHotkeyReleaseAsync(int triggerVirtualKey, CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow.AddSeconds(1);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsDown(VirtualKeyControl) && !IsDown(VirtualKeyShift) &&
                !IsDown(VirtualKeyAlt) && !IsDown(VirtualKeyLeftWindows) &&
                !IsDown(VirtualKeyRightWindows) && !IsDown((ushort)triggerVirtualKey))
            {
                return;
            }

            await Task.Delay(15, cancellationToken);
        }
    }

    public void SendCopy() => SendControlShortcut(VirtualKeyC);

    public void SendPaste() => SendControlShortcut(VirtualKeyV);

    private static bool IsDown(ushort virtualKey) => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;

    private static void SendControlShortcut(ushort virtualKey)
    {
        var inputs = new[]
        {
            CreateKeyboardInput(VirtualKeyControl, keyUp: false),
            CreateKeyboardInput(virtualKey, keyUp: false),
            CreateKeyboardInput(virtualKey, keyUp: true),
            CreateKeyboardInput(VirtualKeyControl, keyUp: true)
        };

        var sent = SendInput((uint)inputs.Length, inputs, NativeInputSize);
        if (sent != inputs.Length)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not simulate keyboard input.");
        }
    }

    private static Input CreateKeyboardInput(ushort virtualKey, bool keyUp) => new()
    {
        Type = InputKeyboard,
        Union = new InputUnion
        {
            Keyboard = new KeyboardInput
            {
                VirtualKey = virtualKey,
                Flags = keyUp ? KeyEventKeyUp : 0
            }
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Union;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mouse;

        [FieldOffset(0)]
        public KeyboardInput Keyboard;

        [FieldOffset(0)]
        public HardwareInput Hardware;
    }

    // The unused MOUSEINPUT and HARDWAREINPUT members are required so the union
    // has the exact native INPUT size (40 bytes on x64, 28 bytes on x86).
    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HardwareInput
    {
        public uint Message;
        public ushort ParameterLow;
        public ushort ParameterHigh;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);
}
