using SwitchBack.Core;

namespace SwitchBack.Tests;

public sealed class MixedLayoutTextConverterTests
{
    private readonly MixedLayoutTextConverter _converter = new();
    private readonly KeyboardMapper _keyboardMapper = new();

    [Fact]
    public void TargetLanguageOnlyPreservesTextAlreadyInTargetLayout()
    {
        var result = _converter.Convert(
            "l;ylfu สวัสดี",
            CreateMapper(englishToThai: true),
            CreateMapper(englishToThai: false),
            LayoutConversionBehavior.TargetLanguageOnly);

        Assert.Equal("สวัสดี สวัสดี", result.Output);
    }

    [Fact]
    public void SwapBothLayoutsConvertsEachScriptInTheOppositeDirection()
    {
        var result = _converter.Convert(
            "l;ylfu สวัสดี",
            CreateMapper(englishToThai: true),
            CreateMapper(englishToThai: false),
            LayoutConversionBehavior.SwapBothLayouts);

        Assert.Equal("สวัสดี l;ylfu", result.Output);
    }

    [Fact]
    public void PreservesWhitespaceEmojiAndUnsupportedCharacters()
    {
        var result = _converter.Convert(
            "l;ylfu  😀",
            CreateMapper(englishToThai: true),
            CreateMapper(englishToThai: false),
            LayoutConversionBehavior.TargetLanguageOnly);

        Assert.Equal("สวัสดี  😀", result.Output);
    }

    private ICharacterLayoutMapper CreateMapper(bool englishToThai) => new DelegateMapper(
        englishToThai ? "en-US" : "th-TH",
        englishToThai ? "th-TH" : "en-US",
        englishToThai
            ? _keyboardMapper.TryMapEnglishToThai
            : _keyboardMapper.TryMapThaiToEnglish);

    private delegate bool TryMapCharacter(char input, out char output);

    private sealed class DelegateMapper(
        string sourceLayoutId,
        string targetLayoutId,
        TryMapCharacter tryMap) : ICharacterLayoutMapper
    {
        public string SourceLayoutId { get; } = sourceLayoutId;

        public string TargetLayoutId { get; } = targetLayoutId;

        public bool TryMap(char input, out string output)
        {
            if (tryMap(input, out var mapped))
            {
                output = mapped.ToString();
                return true;
            }

            output = string.Empty;
            return false;
        }
    }
}
