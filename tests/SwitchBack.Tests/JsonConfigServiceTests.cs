using SwitchBack.Config;

namespace SwitchBack.Tests;

public sealed class JsonConfigServiceTests
{
    [Fact]
    public void MigratesLegacySettingsToFollowWindowsLanguage()
    {
        var path = CreateTemporaryConfigPath();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, """
                {
                  "Enabled": true,
                  "ConversionMode": "Auto"
                }
                """);

            var settings = new JsonConfigService(path).Load();

            Assert.Equal(2, settings.SchemaVersion);
            Assert.Equal(ConversionMode.FollowWindowsLanguage, settings.ConversionMode);
            Assert.Equal(MixedTextPolicy.TargetLanguageOnly, settings.MixedTextPolicy);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public void SavesAndLoadsLanguageAndLayoutPreferences()
    {
        var path = CreateTemporaryConfigPath();

        try
        {
            var service = new JsonConfigService(path);
            var expected = new AppSettings
            {
                UiLanguage = UiLanguageMode.Thai,
                MixedTextPolicy = MixedTextPolicy.SwapBothLayouts,
                InputLayouts = new InputLayoutSettings
                {
                    LayoutAId = "A",
                    LayoutBId = "B"
                }
            };

            service.Save(expected);
            var actual = service.Load();

            Assert.Equal(UiLanguageMode.Thai, actual.UiLanguage);
            Assert.Equal(MixedTextPolicy.SwapBothLayouts, actual.MixedTextPolicy);
            Assert.Equal("A", actual.InputLayouts.LayoutAId);
            Assert.Equal("B", actual.InputLayouts.LayoutBId);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    private static string CreateTemporaryConfigPath() => Path.Combine(
        Path.GetTempPath(),
        "SwitchBack.Tests",
        Guid.NewGuid().ToString("N"),
        "settings.json");

    private static void DeleteTemporaryDirectory(string configPath)
    {
        var directory = Path.GetDirectoryName(configPath);
        if (directory is not null && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
