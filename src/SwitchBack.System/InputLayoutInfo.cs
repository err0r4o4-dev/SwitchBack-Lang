namespace SwitchBack.SystemServices;

public sealed record InputLayoutInfo(
    string Id,
    long HandleValue,
    string LanguageTag,
    string DisplayName,
    InputLayoutCapability Capability)
{
    public IntPtr Handle => new(HandleValue);

    public bool IsSupported => Capability == InputLayoutCapability.DirectKeyboard;

    public override string ToString()
    {
        var capability = Capability switch
        {
            InputLayoutCapability.DirectKeyboard => "Generic",
            InputLayoutCapability.InputMethodEditor => "IME — unsupported",
            _ => "Unknown"
        };

        return $"{DisplayName} ({LanguageTag}) — {capability}";
    }
}
