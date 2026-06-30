namespace SwitchBack.Config;

public sealed class HotkeySettings
{
    public bool Control { get; set; } = true;

    public bool Shift { get; set; } = true;

    public bool Alt { get; set; }

    public bool Windows { get; set; }

    public string Key { get; set; } = "Space";

    public HotkeySettings Clone() => new()
    {
        Control = Control,
        Shift = Shift,
        Alt = Alt,
        Windows = Windows,
        Key = Key
    };

    public override string ToString()
    {
        var parts = new List<string>();
        if (Control) parts.Add("Ctrl");
        if (Shift) parts.Add("Shift");
        if (Alt) parts.Add("Alt");
        if (Windows) parts.Add("Win");
        parts.Add(Key);
        return string.Join(" + ", parts);
    }
}
