namespace SwitchBack.Core;

public sealed record LayoutConversionResult(
    string Input,
    string Output,
    string SourceLayoutId,
    string TargetLayoutId,
    int ConvertedCharacterCount)
{
    public bool Changed => !string.Equals(Input, Output, StringComparison.Ordinal);
}
