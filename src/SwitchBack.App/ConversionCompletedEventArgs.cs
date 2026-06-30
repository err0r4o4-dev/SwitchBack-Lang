namespace SwitchBack.App;

public sealed class ConversionCompletedEventArgs(int convertedCharacterCount) : EventArgs
{
    public int ConvertedCharacterCount { get; } = convertedCharacterCount;
}
