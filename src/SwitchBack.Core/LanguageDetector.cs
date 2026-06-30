namespace SwitchBack.Core;

public sealed class LanguageDetector
{
    public ConversionDirection Detect(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var englishScore = 0;
        var thaiScore = 0;

        foreach (var character in text)
        {
            if (IsThai(character))
            {
                thaiScore++;
            }
            else if (IsEnglishLetter(character))
            {
                englishScore++;
            }
        }

        return thaiScore > 0 && thaiScore >= englishScore
            ? ConversionDirection.ThaiToEnglish
            : ConversionDirection.EnglishToThai;
    }

    private static bool IsEnglishLetter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    private static bool IsThai(char character) =>
        character is >= '\u0E00' and <= '\u0E7F';
}
