using Microsoft.Win32;

namespace SwitchBack.SystemServices;

public sealed class StartupService
{
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "SwitchBack";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        return key?.GetValue(ValueName) is string;
    }

    public void SetEnabled(bool enabled, string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);

        using var key = Registry.CurrentUser.CreateSubKey(RegistryPath, writable: true);
        if (enabled)
        {
            key.SetValue(ValueName, $"\"{executablePath}\" --background", RegistryValueKind.String);
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}
