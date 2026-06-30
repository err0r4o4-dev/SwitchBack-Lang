using System.Text;

namespace SwitchBack.Core;

public sealed class TextConverter
{
    private readonly KeyboardMapper _keyboardMapper;
    private readonly LanguageDetector _languageDetector;

    public TextConverter(KeyboardMapper keyboardMapper, LanguageDetector languageDetector)
    {
        _keyboardMapper = keyboardMapper;
        _languageDetector = languageDetector;
    }

    public ConversionResult Convert(string input, ConversionDirection direction = ConversionDirection.Auto)
    {
        ArgumentNullException.ThrowIfNull(input);

        var resolvedDirection = direction == ConversionDirection.Auto
            ? _languageDetector.Detect(input)
            : direction;

        var output = new StringBuilder(input.Length);
        var convertedCharacterCount = 0;

        foreach (var character in input)
        {
            var mapped = resolvedDirection == ConversionDirection.EnglishToThai
                ? _keyboardMapper.TryMapEnglishToThai(character, out var converted)
                : _keyboardMapper.TryMapThaiToEnglish(character, out converted);

            if (mapped)
            {
                output.Append(converted);
                convertedCharacterCount++;
            }
            else
            {
                output.Append(character);
            }
        }

        return new ConversionResult(input, output.ToString(), resolvedDirection, convertedCharacterCount);
    }
}
