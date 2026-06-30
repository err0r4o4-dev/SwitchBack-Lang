namespace SwitchBack.Config;

public sealed class UserPreferences
{
    public bool MinimizeToTray { get; set; } = true;

    public bool ShowNotifications { get; set; } = true;

    public bool StartWithWindows { get; set; }

    public UserPreferences Clone() => new()
    {
        MinimizeToTray = MinimizeToTray,
        ShowNotifications = ShowNotifications,
        StartWithWindows = StartWithWindows
    };
}
