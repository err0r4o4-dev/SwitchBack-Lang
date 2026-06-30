using System.Globalization;
using System.Runtime.InteropServices;

namespace SwitchBack.SystemServices;

public sealed class WindowsInputLanguageService
{
    public IReadOnlyList<InputLayoutInfo> GetInstalledLayouts()
    {
        var count = GetKeyboardLayoutList(0, null);
        if (count <= 0)
        {
            return Array.Empty<InputLayoutInfo>();
        }

        var handles = new IntPtr[count];
        var copied = GetKeyboardLayoutList(handles.Length, handles);

        return handles
            .Take(copied)
            .Distinct()
            .Select(CreateInfo)
            .OrderBy(layout => layout.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public InputLayoutInfo? GetForegroundLayout()
    {
        var foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero)
        {
            return null;
        }

        var threadId = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
        var handle = GetKeyboardLayout(threadId);
        return handle == IntPtr.Zero ? null : CreateInfo(handle);
    }

    public InputLayoutInfo? FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return GetInstalledLayouts().FirstOrDefault(
            layout => string.Equals(layout.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    private static InputLayoutInfo CreateInfo(IntPtr handle)
    {
        var value = handle.ToInt64();
        var languageId = unchecked((ushort)(value & 0xFFFF));
        var languageTag = "und";
        var displayName = $"Keyboard layout 0x{languageId:X4}";

        try
        {
            var culture = CultureInfo.GetCultureInfo(languageId);
            languageTag = string.IsNullOrWhiteSpace(culture.Name) ? "und" : culture.Name;
            displayName = culture.NativeName;
        }
        catch (CultureNotFoundException)
        {
        }

        var capability = ImmIsIME(handle)
            ? InputLayoutCapability.InputMethodEditor
            : InputLayoutCapability.DirectKeyboard;

        return new InputLayoutInfo(
            value.ToString("X16", CultureInfo.InvariantCulture),
            value,
            languageTag,
            displayName,
            capability);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetKeyboardLayoutList(int bufferSize, [Out] IntPtr[]? layouts);

    [DllImport("user32.dll")]
    private static extern IntPtr GetKeyboardLayout(uint threadId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr windowHandle, IntPtr processId);

    [DllImport("imm32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ImmIsIME(IntPtr keyboardLayout);
}
