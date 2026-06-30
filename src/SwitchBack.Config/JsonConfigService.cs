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
                return new AppSettings();
            }

            var json = File.ReadAllText(ConfigPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _serializerOptions);
            return Normalize(settings ?? new AppSettings());
        }
        catch (IOException)
        {
            return new AppSettings();
        }
        catch (UnauthorizedAccessException)
        {
            return new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
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
        settings.Hotkey ??= new HotkeySettings();
        settings.Preferences ??= new UserPreferences();
        settings.Hotkey.Key = string.IsNullOrWhiteSpace(settings.Hotkey.Key)
            ? "Space"
            : settings.Hotkey.Key.Trim();
        settings.ClipboardRestoreDelayMs = Math.Clamp(settings.ClipboardRestoreDelayMs, 100, 2_000);
        return settings;
    }
}
