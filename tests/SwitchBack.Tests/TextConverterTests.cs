using SwitchBack.Core;

namespace SwitchBack.Tests;

public sealed class TextConverterTests
{
    private readonly TextConverter _converter = new(new KeyboardMapper(), new LanguageDetector());

    [Fact]
    public void ConvertsMistypedEnglishKeysToThaiKedmanee()
    {
        var result = _converter.Convert("l;ylfu8iy[", ConversionDirection.EnglishToThai);

        Assert.Equal("สวัสดีครับ", result.Output);
        Assert.True(result.Changed);
        Assert.Equal(10, result.ConvertedCharacterCount);
    }

    [Fact]
    public void ConvertsThaiKedmaneeBackToEnglishKeys()
    {
        var result = _converter.Convert("สวัสดีครับ", ConversionDirection.ThaiToEnglish);

        Assert.Equal("l;ylfu8iy[", result.Output);
        Assert.Equal(ConversionDirection.ThaiToEnglish, result.Direction);
    }

    [Fact]
    public void PreservesSpacesAndUnsupportedCharacters()
    {
        var result = _converter.Convert("l;ylfu  😀", ConversionDirection.EnglishToThai);

        Assert.Equal("สวัสดี  😀", result.Output);
    }

    [Fact]
    public void AutoModeChoosesEnglishToThaiForMistypedLatinText()
    {
        var result = _converter.Convert("l;ylfu8iy[");

        Assert.Equal(ConversionDirection.EnglishToThai, result.Direction);
        Assert.Equal("สวัสดีครับ", result.Output);
    }

    [Fact]
    public void AutoModeChoosesThaiToEnglishForThaiText()
    {
        var result = _converter.Convert("สวัสดีครับ");

        Assert.Equal(ConversionDirection.ThaiToEnglish, result.Direction);
        Assert.Equal("l;ylfu8iy[", result.Output);
    }
}
