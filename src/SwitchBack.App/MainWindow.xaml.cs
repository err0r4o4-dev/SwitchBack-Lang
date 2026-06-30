using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using SwitchBack.Config;
using SwitchBack.SystemServices;

namespace SwitchBack.App;

public partial class MainWindow : Window
{
    private readonly LocalizationService _localization;
    private readonly WindowsInputLanguageService _inputLanguageService;
    private AppSettings _settings = new();
    private IReadOnlyList<InputLayoutInfo> _availableLayouts = Array.Empty<InputLayoutInfo>();
    private bool _closeForExit;

    public MainWindow(
        LocalizationService localization,
        WindowsInputLanguageService inputLanguageService)
    {
        _localization = localization;
        _inputLanguageService = inputLanguageService;

        InitializeComponent();
        HotkeyComboBox.ItemsSource = BuildHotkeyChoices();
        ApplyLocalization();
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
        _availableLayouts = _inputLanguageService.GetInstalledLayouts();

        RefreshLocalizedOptions();
        LayoutAComboBox.ItemsSource = _availableLayouts;
        LayoutBComboBox.ItemsSource = _availableLayouts;

        EnabledCheckBox.IsChecked = settings.Enabled;
        SelectOption(UiLanguageComboBox, settings.UiLanguage);
        SelectOption(ConversionModeComboBox, settings.ConversionMode);
        SelectOption(MixedTextPolicyComboBox, settings.MixedTextPolicy);
        LayoutAComboBox.SelectedItem = FindLayout(settings.InputLayouts.LayoutAId);
        LayoutBComboBox.SelectedItem = FindLayout(settings.InputLayouts.LayoutBId);
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

    public void ApplyLocalization()
    {
        Title = _localization["WindowTitle"];
        SubtitleTextBlock.Text = _localization["Subtitle"];
        InterfaceLanguageGroup.Header = _localization["InterfaceLanguage"];
        StatusGroup.Header = _localization["Status"];
        EnabledCheckBox.Content = _localization["EnableConversion"];
        UsageHintTextBlock.Text = _localization["UsageHint"];
        ConversionGroup.Header = _localization["Conversion"];
        DirectionLabel.Text = _localization["Direction"];
        LayoutALabel.Text = _localization["LayoutA"];
        LayoutBLabel.Text = _localization["LayoutB"];
        MixedTextLabel.Text = _localization["MixedText"];
        RestoreClipboardCheckBox.Content = _localization["RestoreClipboard"];
        GlobalHotkeyGroup.Header = _localization["GlobalHotkey"];
        HotkeyHintTextBlock.Text = _localization["HotkeyHint"];
        WindowsGroup.Header = _localization["Windows"];
        StartWithWindowsCheckBox.Content = _localization["StartWithWindows"];
        NotificationsCheckBox.Content = _localization["Notifications"];
        PrivacyTitleRun.Text = _localization["PrivacyTitle"];
        PrivacyRun.Text = _localization["Privacy"];
        HideButton.Content = _localization["HideToTray"];
        SaveButton.Content = _localization["SaveSettings"];
        RefreshLocalizedOptions();
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
        updated.UiLanguage = SelectedValue(UiLanguageComboBox, UiLanguageMode.System);
        updated.ConversionMode = SelectedValue(ConversionModeComboBox, ConversionMode.FollowWindowsLanguage);
        updated.MixedTextPolicy = SelectedValue(MixedTextPolicyComboBox, MixedTextPolicy.TargetLanguageOnly);
        updated.InputLayouts.LayoutAId = (LayoutAComboBox.SelectedItem as InputLayoutInfo)?.Id ?? string.Empty;
        updated.InputLayouts.LayoutBId = (LayoutBComboBox.SelectedItem as InputLayoutInfo)?.Id ?? string.Empty;
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
        ApplyLocalization();
        StatusTextBlock.Foreground = System.Windows.Media.Brushes.SeaGreen;
        StatusTextBlock.Text = _localization["SettingsSaved"];
    }

    private void HideButton_Click(object sender, RoutedEventArgs e) => Hide();

    private void RefreshLocalizedOptions()
    {
        var selectedUiLanguage = SelectedValue(UiLanguageComboBox, _settings.UiLanguage);
        var selectedMode = SelectedValue(ConversionModeComboBox, _settings.ConversionMode);
        var selectedPolicy = SelectedValue(MixedTextPolicyComboBox, _settings.MixedTextPolicy);

        UiLanguageComboBox.ItemsSource = new[]
        {
            new SelectionOption<UiLanguageMode>(UiLanguageMode.System, _localization["SystemLanguage"]),
            new SelectionOption<UiLanguageMode>(UiLanguageMode.English, "English"),
            new SelectionOption<UiLanguageMode>(UiLanguageMode.Thai, "ไทย")
        };
        ConversionModeComboBox.ItemsSource = Enum.GetValues<ConversionMode>()
            .Select(mode => new SelectionOption<ConversionMode>(mode, _localization.GetConversionModeName(mode)))
            .ToArray();
        MixedTextPolicyComboBox.ItemsSource = Enum.GetValues<MixedTextPolicy>()
            .Select(policy => new SelectionOption<MixedTextPolicy>(policy, _localization.GetMixedTextPolicyName(policy)))
            .ToArray();

        SelectOption(UiLanguageComboBox, selectedUiLanguage);
        SelectOption(ConversionModeComboBox, selectedMode);
        SelectOption(MixedTextPolicyComboBox, selectedPolicy);
    }

    private InputLayoutInfo? FindLayout(string id) => _availableLayouts.FirstOrDefault(
        layout => string.Equals(layout.Id, id, StringComparison.OrdinalIgnoreCase));

    private static T SelectedValue<T>(System.Windows.Controls.ComboBox comboBox, T fallback)
        where T : struct, Enum => comboBox.SelectedItem is SelectionOption<T> option ? option.Value : fallback;

    private static void SelectOption<T>(System.Windows.Controls.ComboBox comboBox, T value)
        where T : struct, Enum => comboBox.SelectedItem = comboBox.Items
            .OfType<SelectionOption<T>>()
            .FirstOrDefault(option => EqualityComparer<T>.Default.Equals(option.Value, value));

    private static IReadOnlyList<string> BuildHotkeyChoices()
    {
        var keys = new List<string> { "Space", "Enter", "Tab" };
        keys.AddRange(Enumerable.Range('A', 26).Select(character => ((char)character).ToString()));
        keys.AddRange(Enumerable.Range(1, 12).Select(number => $"F{number}"));
        return keys;
    }

    private sealed record SelectionOption<T>(T Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }
}
