using SwitchBack.Core;

namespace SwitchBack.Tests;

public sealed class LanguageDetectorTests
{
    private readonly LanguageDetector _detector = new();

    [Theory]
    [InlineData("hello world", ConversionDirection.EnglishToThai)]
    [InlineData("l;ylfu8iy[", ConversionDirection.EnglishToThai)]
    [InlineData("สวัสดีครับ", ConversionDirection.ThaiToEnglish)]
    [InlineData("ไทย 123 abc", ConversionDirection.ThaiToEnglish)]
    public void DetectsDirectionFromDominantScript(string input, ConversionDirection expected)
    {
        Assert.Equal(expected, _detector.Detect(input));
    }
}
