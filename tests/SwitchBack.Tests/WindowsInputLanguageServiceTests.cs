using System.Text;
using SwitchBack.SystemServices;

namespace SwitchBack.Tests;

public sealed class WindowsInputLanguageServiceTests
{
    private readonly WindowsInputLanguageService _service = new();

    [Fact]
    public void InstalledLayoutsHaveStableUniqueIds()
    {
        var layouts = _service.GetInstalledLayouts();

        Assert.NotEmpty(layouts);
        Assert.Equal(layouts.Count, layouts.Select(layout => layout.Id).Distinct().Count());
    }

    [Fact]
    public void GenericMapperUsesInstalledEnglishAndThaiLayoutsWhenAvailable()
    {
        var layouts = _service.GetInstalledLayouts();
        var english = layouts.FirstOrDefault(layout =>
            layout.IsSupported && layout.LanguageTag.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        var thai = layouts.FirstOrDefault(layout =>
            layout.IsSupported && layout.LanguageTag.StartsWith("th", StringComparison.OrdinalIgnoreCase));

        if (english is null || thai is null)
        {
            return;
        }

        var mapper = new WindowsLayoutCharacterMapper(english, thai);
        var output = new StringBuilder();

        foreach (var character in "l;ylfu")
        {
            output.Append(mapper.TryMap(character, out var mapped) ? mapped : character);
        }

        Assert.Equal("สวัสดี", output.ToString());
    }
}
