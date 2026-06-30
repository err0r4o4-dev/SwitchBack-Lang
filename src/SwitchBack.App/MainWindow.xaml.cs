using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using SwitchBack.Config;
using SwitchBack.SystemServices;

namespace SwitchBack.App;

public partial class MainWindow : Window
{
    private AppSettings _settings = new();
    private bool _closeForExit;

    public MainWindow()
    {
        InitializeComponent();

        ConversionModeComboBox.ItemsSource = Enum.GetValues<ConversionMode>();
        HotkeyComboBox.ItemsSource = BuildHotkeyChoices();
    }

    public Func<AppSettings, string?>? SaveSettings { get; init; }

    public IntPtr WindowHandle { get; private set; }

    public void AttachHotkeyService(GlobalHotkeyService hotkeyService)
    {
        ArgumentNullException.ThrowIfNull(hotkeyService);

        WindowHandle = new WindowInteropHelper(this).EnsureHandle();
        var source = HwndSource.FromHwnd(WindowHandle)
            ?? throw new InvalidOperationException("Could not create the Windows message source.");

        source.AddHook((IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam, ref bool handled) =>
        {
            handled = hotkeyService.ProcessWindowMessage(message, wParam);
            return IntPtr.Zero;
        });
    }

    public void LoadSettings(AppSettings settings)
    {
        _settings = settings.Clone();

        EnabledCheckBox.IsChecked = settings.Enabled;
        ConversionModeComboBox.SelectedItem = settings.ConversionMode;
        RestoreClipboardCheckBox.IsChecked = settings.RestoreClipboard;
        ControlCheckBox.IsChecked = settings.Hotkey.Control;
        ShiftCheckBox.IsChecked = settings.Hotkey.Shift;
        AltCheckBox.IsChecked = settings.Hotkey.Alt;
        WindowsCheckBox.IsChecked = settings.Hotkey.Windows;
        HotkeyComboBox.SelectedItem = settings.Hotkey.Key;
        StartWithWindowsCheckBox.IsChecked = settings.Preferences.StartWithWindows;
        NotificationsCheckBox.IsChecked = settings.Preferences.ShowNotifications;
        StatusTextBlock.Text = string.Empty;
    }

    public void CloseForExit()
    {
        _closeForExit = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_closeForExit)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnClosing(e);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var updated = _settings.Clone();
        updated.Enabled = EnabledCheckBox.IsChecked == true;
        updated.ConversionMode = ConversionModeComboBox.SelectedItem is ConversionMode mode
            ? mode
            : ConversionMode.Auto;
        updated.RestoreClipboard = RestoreClipboardCheckBox.IsChecked == true;
        updated.Hotkey.Control = ControlCheckBox.IsChecked == true;
        updated.Hotkey.Shift = ShiftCheckBox.IsChecked == true;
        updated.Hotkey.Alt = AltCheckBox.IsChecked == true;
        updated.Hotkey.Windows = WindowsCheckBox.IsChecked == true;
        updated.Hotkey.Key = HotkeyComboBox.SelectedItem?.ToString() ?? "Space";
        updated.Preferences.StartWithWindows = StartWithWindowsCheckBox.IsChecked == true;
        updated.Preferences.ShowNotifications = NotificationsCheckBox.IsChecked == true;

        var error = SaveSettings?.Invoke(updated);
        if (error is not null)
        {
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Firebrick;
            StatusTextBlock.Text = error;
            return;
        }

        _settings = updated;
        StatusTextBlock.Foreground = System.Windows.Media.Brushes.SeaGreen;
        StatusTextBlock.Text = "Settings saved.";
    }

    private void HideButton_Click(object sender, RoutedEventArgs e) => Hide();

    private static IReadOnlyList<string> BuildHotkeyChoices()
    {
        var keys = new List<string> { "Space", "Enter", "Tab" };
        keys.AddRange(Enumerable.Range('A', 26).Select(character => ((char)character).ToString()));
        keys.AddRange(Enumerable.Range(1, 12).Select(number => $"F{number}"));
        return keys;
    }
}
