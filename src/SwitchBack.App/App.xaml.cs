using System.IO;
using System.Threading;
using System.Windows;
using SwitchBack.Config;
using SwitchBack.Core;
using SwitchBack.SystemServices;

namespace SwitchBack.App;

public partial class App : System.Windows.Application
{
    private Mutex? _singleInstanceMutex;
    private MainWindow? _mainWindow;
    private TrayIconService? _trayIcon;
    private GlobalHotkeyService? _hotkeyService;
    private ConversionCoordinator? _conversionCoordinator;
    private JsonConfigService? _configService;
    private StartupService? _startupService;
    private LocalizationService? _localization;
    private WindowsInputLanguageService? _inputLanguageService;
    private AppSettings _settings = new();
    private bool _isExiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(true, "Local\\SwitchBack-Lang", out var isFirstInstance);
        if (!isFirstInstance)
        {
            System.Windows.MessageBox.Show(
                "SwitchBack is already running in the system tray.",
                "SwitchBack",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        _configService = new JsonConfigService();
        _startupService = new StartupService();
        var hasExistingConfig = File.Exists(_configService.ConfigPath);
        _settings = _configService.Load();
        if (!hasExistingConfig)
        {
            ApplyInstallerLanguageArgument(_settings, e.Args);
        }
        _localization = new LocalizationService();
        _localization.Apply(_settings.UiLanguage);
        _inputLanguageService = new WindowsInputLanguageService();
        EnsureDefaultInputLayouts(_settings, _inputLanguageService.GetInstalledLayouts());

        _hotkeyService = new GlobalHotkeyService();
        _mainWindow = new MainWindow(_localization, _inputLanguageService)
        {
            SaveSettings = ApplySettings
        };
        _mainWindow.LoadSettings(_settings);
        _mainWindow.AttachHotkeyService(_hotkeyService);

        var converter = new TextConverter(new KeyboardMapper(), new LanguageDetector());
        _conversionCoordinator = new ConversionCoordinator(
            converter,
            new MixedLayoutTextConverter(),
            _inputLanguageService,
            new ClipboardService(),
            new KeyboardInputService());

        _trayIcon = new TrayIconService(_localization);
        _trayIcon.ShowSettingsRequested += (_, _) => ShowSettings();
        _trayIcon.EnabledToggleRequested += (_, _) => ToggleEnabled();
        _trayIcon.ExitRequested += (_, _) => ExitApplication();
        _conversionCoordinator.Error += (_, message) => _trayIcon.ShowMessage(_localization["ConversionFailed"], message, isError: true);
        _conversionCoordinator.Converted += (_, result) =>
        {
            if (_settings.Preferences.ShowNotifications)
            {
                _trayIcon.ShowMessage(
                    _localization["TextConverted"],
                    string.Format(_localization["CharactersConverted"], result.ConvertedCharacterCount));
            }
        };

        _hotkeyService.HotkeyPressed += HotkeyPressed;

        var startupError = ApplySettings(_settings, persist: false);
        if (startupError is not null)
        {
            _trayIcon.ShowMessage(_localization["NeedsAttention"], startupError, isError: true);
        }

        _trayIcon.SetEnabled(_settings.Enabled);

        if (!e.Args.Contains("--background", StringComparer.OrdinalIgnoreCase))
        {
            ShowSettings();
        }
    }

    private async void HotkeyPressed(object? sender, EventArgs e)
    {
        if (!_settings.Enabled || _conversionCoordinator is null)
        {
            return;
        }

        await _conversionCoordinator.ConvertSelectionAsync(_settings.Clone());
    }

    private string? ApplySettings(AppSettings newSettings) => ApplySettings(newSettings, persist: true);

    private string? ApplySettings(AppSettings newSettings, bool persist)
    {
        if (_mainWindow is null || _hotkeyService is null || _configService is null ||
            _startupService is null || _localization is null || _inputLanguageService is null)
        {
            return _localization?["ServicesNotReady"] ?? "Application services are not ready.";
        }

        if (!newSettings.Hotkey.Control && !newSettings.Hotkey.Shift &&
            !newSettings.Hotkey.Alt && !newSettings.Hotkey.Windows)
        {
            return _localization["SelectModifiers"];
        }

        if (newSettings.ConversionMode == ConversionMode.FollowWindowsLanguage)
        {
            var layoutA = _inputLanguageService.FindById(newSettings.InputLayouts.LayoutAId);
            var layoutB = _inputLanguageService.FindById(newSettings.InputLayouts.LayoutBId);
            if (layoutA is null || layoutB is null || !layoutA.IsSupported || !layoutB.IsSupported ||
                string.Equals(layoutA.Id, layoutB.Id, StringComparison.OrdinalIgnoreCase))
            {
                return _localization["SelectTwoLayouts"];
            }
        }

        var previousSettings = _settings.Clone();

        try
        {
            if (newSettings.Enabled)
            {
                _hotkeyService.Register(_mainWindow.WindowHandle, newSettings.Hotkey);
            }
            else
            {
                _hotkeyService.Unregister();
            }

            var executablePath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(executablePath))
            {
                _startupService.SetEnabled(newSettings.Preferences.StartWithWindows, executablePath);
            }

            _settings = newSettings.Clone();
            _localization.Apply(_settings.UiLanguage);
            if (persist)
            {
                _configService.Save(_settings);
            }

            _trayIcon?.SetEnabled(_settings.Enabled);
            _trayIcon?.ApplyLocalization();
            _mainWindow.ApplyLocalization();
            return null;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
        {
            TryRestorePreviousHotkey(previousSettings);
            return exception.Message;
        }
    }

    private void TryRestorePreviousHotkey(AppSettings previousSettings)
    {
        if (_hotkeyService is null || _mainWindow is null)
        {
            return;
        }

        try
        {
            if (previousSettings.Enabled)
            {
                _hotkeyService.Register(_mainWindow.WindowHandle, previousSettings.Hotkey);
            }
            else
            {
                _hotkeyService.Unregister();
            }
        }
        catch
        {
            _hotkeyService.Unregister();
        }
    }

    private void ToggleEnabled()
    {
        var changedSettings = _settings.Clone();
        changedSettings.Enabled = !changedSettings.Enabled;
        var error = ApplySettings(changedSettings);

        if (error is not null)
        {
            _trayIcon?.ShowMessage(_localization?["CouldNotChangeStatus"] ?? "Could not change status", error, isError: true);
        }

        _mainWindow?.LoadSettings(_settings);
    }

    private void ShowSettings()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.LoadSettings(_settings);
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
        _mainWindow?.CloseForExit();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private static void EnsureDefaultInputLayouts(
        AppSettings settings,
        IReadOnlyList<InputLayoutInfo> installedLayouts)
    {
        var supported = installedLayouts.Where(layout => layout.IsSupported).ToArray();
        var existingA = supported.FirstOrDefault(layout =>
            string.Equals(layout.Id, settings.InputLayouts.LayoutAId, StringComparison.OrdinalIgnoreCase));
        var existingB = supported.FirstOrDefault(layout =>
            string.Equals(layout.Id, settings.InputLayouts.LayoutBId, StringComparison.OrdinalIgnoreCase));

        if (existingA is not null && existingB is not null && existingA.Id != existingB.Id)
        {
            return;
        }

        var english = supported.FirstOrDefault(layout => layout.LanguageTag.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        var thai = supported.FirstOrDefault(layout => layout.LanguageTag.StartsWith("th", StringComparison.OrdinalIgnoreCase));

        if (english is not null && thai is not null)
        {
            settings.InputLayouts.LayoutAId = english.Id;
            settings.InputLayouts.LayoutBId = thai.Id;
            return;
        }

        if (supported.Length >= 2)
        {
            settings.InputLayouts.LayoutAId = supported[0].Id;
            settings.InputLayouts.LayoutBId = supported[1].Id;
            return;
        }

        settings.InputLayouts.LayoutAId = supported.FirstOrDefault()?.Id ?? string.Empty;
        settings.InputLayouts.LayoutBId = string.Empty;
        if (settings.ConversionMode == ConversionMode.FollowWindowsLanguage)
        {
            settings.Enabled = false;
        }
    }

    private static void ApplyInstallerLanguageArgument(AppSettings settings, IReadOnlyList<string> arguments)
    {
        var languageArgument = arguments.FirstOrDefault(argument =>
            argument.StartsWith("--ui-language=", StringComparison.OrdinalIgnoreCase));
        var language = languageArgument?.Split('=', 2).ElementAtOrDefault(1);

        settings.UiLanguage = language?.ToLowerInvariant() switch
        {
            "thai" => UiLanguageMode.Thai,
            "english" => UiLanguageMode.English,
            _ => settings.UiLanguage
        };
    }
}
