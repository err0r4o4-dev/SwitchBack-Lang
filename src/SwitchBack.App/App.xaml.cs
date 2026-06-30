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
        _settings = _configService.Load();

        _hotkeyService = new GlobalHotkeyService();
        _mainWindow = new MainWindow
        {
            SaveSettings = ApplySettings
        };
        _mainWindow.LoadSettings(_settings);
        _mainWindow.AttachHotkeyService(_hotkeyService);

        var converter = new TextConverter(new KeyboardMapper(), new LanguageDetector());
        _conversionCoordinator = new ConversionCoordinator(
            converter,
            new ClipboardService(),
            new KeyboardInputService());

        _trayIcon = new TrayIconService();
        _trayIcon.ShowSettingsRequested += (_, _) => ShowSettings();
        _trayIcon.EnabledToggleRequested += (_, _) => ToggleEnabled();
        _trayIcon.ExitRequested += (_, _) => ExitApplication();
        _conversionCoordinator.Error += (_, message) => _trayIcon.ShowMessage("Conversion failed", message, isError: true);
        _conversionCoordinator.Converted += (_, result) =>
        {
            if (_settings.Preferences.ShowNotifications)
            {
                _trayIcon.ShowMessage("Text converted", $"{result.ConvertedCharacterCount} characters converted.");
            }
        };

        _hotkeyService.HotkeyPressed += HotkeyPressed;

        var startupError = ApplySettings(_settings, persist: false);
        if (startupError is not null)
        {
            _trayIcon.ShowMessage("SwitchBack needs attention", startupError, isError: true);
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
        if (_mainWindow is null || _hotkeyService is null || _configService is null || _startupService is null)
        {
            return "Application services are not ready.";
        }

        if (!newSettings.Hotkey.Control && !newSettings.Hotkey.Shift &&
            !newSettings.Hotkey.Alt && !newSettings.Hotkey.Windows)
        {
            return "Choose at least one modifier key (Ctrl, Shift, Alt, or Win).";
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
            if (persist)
            {
                _configService.Save(_settings);
            }

            _trayIcon?.SetEnabled(_settings.Enabled);
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
            _trayIcon?.ShowMessage("Could not change status", error, isError: true);
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
}
