namespace SwitchBack.Config;

public sealed class InputLayoutSettings
{
    public string LayoutAId { get; set; } = string.Empty;

    public string LayoutBId { get; set; } = string.Empty;

    public InputLayoutSettings Clone() => new()
    {
        LayoutAId = LayoutAId,
        LayoutBId = LayoutBId
    };
}
