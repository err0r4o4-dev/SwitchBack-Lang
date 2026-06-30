namespace SwitchBack.SystemServices;

public sealed record InputLayoutInfo(
    string Id,
    long HandleValue,
    string LanguageTag,
    string DisplayName,
    InputLayoutCapability Capability)
{
    public IntPtr Handle => new(HandleValue);

    public bool IsSupported => Capability is InputLayoutCapability.DirectKeyboard or InputLayoutCapability.InputMethodEditor;

    public override string ToString()
    {
        return $"{DisplayName} ({LanguageTag})";
    }
}
