using SwitchBack.Core;

namespace SwitchBack.Tests;

public sealed class KeyboardMapperTests
{
    private readonly KeyboardMapper _mapper = new();

    [Theory]
    [InlineData('l', 'ส')]
    [InlineData(';', 'ว')]
    [InlineData('8', 'ค')]
    [InlineData('[', 'บ')]
    [InlineData('Q', '๐')]
    public void EnglishKeyMapsToExpectedThaiCharacter(char english, char thai)
    {
        Assert.True(_mapper.TryMapEnglishToThai(english, out var actual));
        Assert.Equal(thai, actual);
    }

    [Theory]
    [InlineData('ส', 'l')]
    [InlineData('ว', ';')]
    [InlineData('ค', '8')]
    [InlineData('บ', '[')]
    [InlineData('๐', 'Q')]
    public void ThaiCharacterMapsToExpectedEnglishKey(char thai, char english)
    {
        Assert.True(_mapper.TryMapThaiToEnglish(thai, out var actual));
        Assert.Equal(english, actual);
    }
}
