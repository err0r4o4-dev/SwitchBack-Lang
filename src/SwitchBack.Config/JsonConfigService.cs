using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwitchBack.Config;

public sealed class JsonConfigService
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonConfigService(string? configPath = null)
    {
        ConfigPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SwitchBack",
            "settings.json");
    }

    public string ConfigPath { get; }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                return Normalize(new AppSettings());
            }

            var json = File.ReadAllText(ConfigPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _serializerOptions);
            using var document = JsonDocument.Parse(json);
            var hasSchemaVersion = document.RootElement.EnumerateObject().Any(property =>
                string.Equals(property.Name, nameof(AppSettings.SchemaVersion), StringComparison.OrdinalIgnoreCase));
            if (settings is not null && !hasSchemaVersion)
            {
                settings.SchemaVersion = 0;
            }

            return Normalize(settings ?? new AppSettings());
        }
        catch (IOException)
        {
            return Normalize(new AppSettings());
        }
        catch (UnauthorizedAccessException)
        {
            return Normalize(new AppSettings());
        }
        catch (JsonException)
        {
            return Normalize(new AppSettings());
        }
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var directory = Path.GetDirectoryName(ConfigPath)
            ?? throw new InvalidOperationException("Config path must include a directory.");
        Directory.CreateDirectory(directory);

        var temporaryPath = ConfigPath + ".tmp";
        var json = JsonSerializer.Serialize(Normalize(settings), _serializerOptions);
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, ConfigPath, true);
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        if (settings.SchemaVersion < 2)
        {
            settings.ConversionMode = ConversionMode.FollowWindowsLanguage;
            settings.MixedTextPolicy = MixedTextPolicy.TargetLanguageOnly;
            settings.SchemaVersion = 2;
        }

        settings.Hotkey ??= new HotkeySettings();
        settings.Preferences ??= new UserPreferences();
        settings.InputLayouts ??= new InputLayoutSettings();
        settings.Hotkey.Key = string.IsNullOrWhiteSpace(settings.Hotkey.Key)
            ? "Space"
            : settings.Hotkey.Key.Trim();
        settings.ClipboardRestoreDelayMs = Math.Clamp(settings.ClipboardRestoreDelayMs, 100, 2_000);
        return settings;
    }
}
