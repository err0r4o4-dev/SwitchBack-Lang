using System.Drawing;
using Forms = System.Windows.Forms;

namespace SwitchBack.App;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ToolStripMenuItem _enabledItem;

    public TrayIconService()
    {
        _enabledItem = new Forms.ToolStripMenuItem("Enabled");
        _enabledItem.Click += (_, _) => EnabledToggleRequested?.Invoke(this, EventArgs.Empty);

        var settingsItem = new Forms.ToolStripMenuItem("Settings");
        settingsItem.Click += (_, _) => ShowSettingsRequested?.Invoke(this, EventArgs.Empty);

        var exitItem = new Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add(settingsItem);
        contextMenu.Items.Add(_enabledItem);
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "SwitchBack",
            Icon = SystemIcons.Application,
            ContextMenuStrip = contextMenu,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => ShowSettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ShowSettingsRequested;

    public event EventHandler? EnabledToggleRequested;

    public event EventHandler? ExitRequested;

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
