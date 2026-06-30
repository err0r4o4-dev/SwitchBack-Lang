using System.Collections.ObjectModel;

namespace SwitchBack.Core;

public sealed class KeyboardMapper
{
    private const string EnglishUnshifted = "`1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./";
    private const string ThaiUnshifted = "_ๅ/-ภถุึคตจขชๆไำพะัีรนยบลฃฟหกดเ้่าสวงผปแอิืทมใฝ";
    private const string EnglishShifted = "~!@#$%^&*()_+QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?";
    private const string ThaiShifted = "%+๑๒๓๔ู฿๕๖๗๘๙๐\"ฎฑธํ๊ณฯญฐ,ฅฤฆฏโฌ็๋ษศซ.()ฉฮฺ์?ฒฬฦ";

    private readonly IReadOnlyDictionary<char, char> _englishToThai;
    private readonly IReadOnlyDictionary<char, char> _thaiToEnglish;

    public KeyboardMapper()
    {
        if (EnglishUnshifted.Length != ThaiUnshifted.Length ||
            EnglishShifted.Length != ThaiShifted.Length)
        {
            throw new InvalidOperationException("Keyboard mapping tables must have equal lengths.");
        }

        var englishToThai = new Dictionary<char, char>();
        var thaiToEnglish = new Dictionary<char, char>();

        AddPairs(EnglishUnshifted, ThaiUnshifted, englishToThai, thaiToEnglish);
        AddPairs(EnglishShifted, ThaiShifted, englishToThai, thaiToEnglish);

        _englishToThai = new ReadOnlyDictionary<char, char>(englishToThai);
        _thaiToEnglish = new ReadOnlyDictionary<char, char>(thaiToEnglish);
    }

    public IReadOnlyDictionary<char, char> EnglishToThaiMap => _englishToThai;

    public IReadOnlyDictionary<char, char> ThaiToEnglishMap => _thaiToEnglish;

    public bool TryMapEnglishToThai(char input, out char output) =>
        _englishToThai.TryGetValue(input, out output);

    public bool TryMapThaiToEnglish(char input, out char output) =>
        _thaiToEnglish.TryGetValue(input, out output);

    private static void AddPairs(
        string english,
        string thai,
        IDictionary<char, char> englishToThai,
        IDictionary<char, char> thaiToEnglish)
    {
        for (var index = 0; index < english.Length; index++)
        {
            englishToThai[english[index]] = thai[index];
            thaiToEnglish[thai[index]] = english[index];
        }
    }
}
