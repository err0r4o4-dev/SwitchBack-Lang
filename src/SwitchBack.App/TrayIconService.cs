using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;

namespace SwitchBack.App;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ToolStripMenuItem _settingsItem;
    private readonly Forms.ToolStripMenuItem _enabledItem;
    private readonly Forms.ToolStripMenuItem _exitItem;
    private readonly LocalizationService _localization;
    private readonly Icon? _brandIcon;

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
        contextMenu.BackColor = Color.FromArgb(16, 18, 22);
        contextMenu.ForeColor = Color.FromArgb(238, 239, 242);
        contextMenu.Renderer = new Forms.ToolStripProfessionalRenderer(new DarkColorTable());
        contextMenu.Items.Add(_settingsItem);
        contextMenu.Items.Add(_enabledItem);
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add(_exitItem);

        _brandIcon = LoadBrandIcon();
        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "SwitchBack",
            Icon = _brandIcon ?? SystemIcons.Application,
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
        _brandIcon?.Dispose();
        GC.SuppressFinalize(this);
    }

    private static Icon? LoadBrandIcon()
    {
        try
        {
            var resource = System.Windows.Application.GetResourceStream(
                new Uri("pack://application:,,,/Assets/SwitchBack-Logo.png"));
            if (resource is null)
            {
                return null;
            }

            using var source = new Bitmap(resource.Stream);
            var side = (int)(Math.Min(source.Width, source.Height) * 0.64);
            var cropX = (source.Width - side) / 2;
            var cropY = (int)(source.Height * 0.16);
            cropY = Math.Clamp(cropY, 0, source.Height - side);

            using var cropped = source.Clone(
                new Rectangle(cropX, cropY, side, side),
                PixelFormat.Format32bppArgb);
            using var resized = new Bitmap(cropped, new Size(64, 64));
            var iconHandle = resized.GetHicon();

            try
            {
                using var temporary = Icon.FromHandle(iconHandle);
                return (Icon)temporary.Clone();
            }
            finally
            {
                DestroyIcon(iconHandle);
            }
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr iconHandle);

    private sealed class DarkColorTable : Forms.ProfessionalColorTable
    {
        private static readonly Color Surface = Color.FromArgb(16, 18, 22);
        private static readonly Color Hover = Color.FromArgb(38, 42, 50);
        private static readonly Color Border = Color.FromArgb(55, 59, 68);

        public override Color ToolStripDropDownBackground => Surface;
        public override Color ImageMarginGradientBegin => Surface;
        public override Color ImageMarginGradientMiddle => Surface;
        public override Color ImageMarginGradientEnd => Surface;
        public override Color MenuBorder => Border;
        public override Color MenuItemBorder => Hover;
        public override Color MenuItemSelected => Hover;
        public override Color MenuItemSelectedGradientBegin => Hover;
        public override Color MenuItemSelectedGradientEnd => Hover;
        public override Color MenuItemPressedGradientBegin => Hover;
        public override Color MenuItemPressedGradientMiddle => Hover;
        public override Color MenuItemPressedGradientEnd => Hover;
        public override Color SeparatorDark => Border;
        public override Color SeparatorLight => Border;
    }
}
