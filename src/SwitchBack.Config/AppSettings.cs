namespace SwitchBack.Config;

public sealed class AppSettings
{
    public int SchemaVersion { get; set; } = 2;

    public bool Enabled { get; set; } = true;

    public UiLanguageMode UiLanguage { get; set; } = UiLanguageMode.System;

    public ConversionMode ConversionMode { get; set; } = ConversionMode.FollowWindowsLanguage;

    public MixedTextPolicy MixedTextPolicy { get; set; } = MixedTextPolicy.TargetLanguageOnly;

    public InputLayoutSettings InputLayouts { get; set; } = new();

    public bool RestoreClipboard { get; set; } = true;

    public int ClipboardRestoreDelayMs { get; set; } = 350;

    public HotkeySettings Hotkey { get; set; } = new();

    public UserPreferences Preferences { get; set; } = new();

    public AppSettings Clone() => new()
    {
        SchemaVersion = SchemaVersion,
        Enabled = Enabled,
        UiLanguage = UiLanguage,
        ConversionMode = ConversionMode,
        MixedTextPolicy = MixedTextPolicy,
        InputLayouts = InputLayouts.Clone(),
        RestoreClipboard = RestoreClipboard,
        ClipboardRestoreDelayMs = ClipboardRestoreDelayMs,
        Hotkey = Hotkey.Clone(),
        Preferences = Preferences.Clone()
    };
}
