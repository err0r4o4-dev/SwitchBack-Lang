using System.Drawing;
using Forms = System.Windows.Forms;

namespace SwitchBack.App;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ToolStripMenuItem _settingsItem;
    private readonly Forms.ToolStripMenuItem _enabledItem;
    private readonly Forms.ToolStripMenuItem _exitItem;
    private readonly LocalizationService _localization;

    public TrayIconService(LocalizationService localization)
    {
        _localization = localization;
        _enabledItem = new Forms.ToolStripMenuItem();
        _enabledItem.Click += (_, _) => EnabledToggleRequested?.Invoke(this, EventArgs.Empty);

        _settingsItem = new Forms.ToolStripMenuItem();
        _settingsItem.Click += (_, _) => ShowSettingsRequested?.Invoke(this, EventArgs.Empty);

        _exitItem = new Forms.ToolStripMenuItem();
        _exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add(_settingsItem);
        contextMenu.Items.Add(_enabledItem);
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add(_exitItem);

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "SwitchBack",
            Icon = SystemIcons.Application,
            ContextMenuStrip = contextMenu,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => ShowSettingsRequested?.Invoke(this, EventArgs.Empty);
        ApplyLocalization();
    }

    public event EventHandler? ShowSettingsRequested;

    public event EventHandler? EnabledToggleRequested;

    public event EventHandler? ExitRequested;

    public void ApplyLocalization()
    {
        _settingsItem.Text = _localization["Settings"];
        _enabledItem.Text = _localization["Enabled"];
        _exitItem.Text = _localization["Exit"];
    }

    public void SetEnabled(bool enabled)
    {
        _enabledItem.Checked = enabled;
        _notifyIcon.Text = enabled ? "SwitchBack — Enabled" : "SwitchBack — Paused";
    }

    public void ShowMessage(string title, string message, bool isError = false)
    {
        _notifyIcon.ShowBalloonTip(
            2_500,
            title,
            message,
            isError ? Forms.ToolTipIcon.Error : Forms.ToolTipIcon.Info);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        GC.SuppressFinalize(this);
    }
}
