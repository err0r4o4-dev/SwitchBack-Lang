namespace SwitchBack.Config;

public sealed class AppSettings
{
    public bool Enabled { get; set; } = true;

    public ConversionMode ConversionMode { get; set; } = ConversionMode.Auto;

    public bool RestoreClipboard { get; set; } = true;

    public int ClipboardRestoreDelayMs { get; set; } = 350;

    public HotkeySettings Hotkey { get; set; } = new();

    public UserPreferences Preferences { get; set; } = new();

    public AppSettings Clone() => new()
    {
        Enabled = Enabled,
        ConversionMode = ConversionMode,
        RestoreClipboard = RestoreClipboard,
        ClipboardRestoreDelayMs = ClipboardRestoreDelayMs,
        Hotkey = Hotkey.Clone(),
        Preferences = Preferences.Clone()
    };
}
