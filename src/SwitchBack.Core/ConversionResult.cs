namespace SwitchBack.Core;

public sealed record ConversionResult(
    string Input,
    string Output,
    ConversionDirection Direction,
    int ConvertedCharacterCount)
{
    public bool Changed => !string.Equals(Input, Output, StringComparison.Ordinal);
}
