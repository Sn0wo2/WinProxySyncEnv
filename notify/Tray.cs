using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using WinProxyEnvSync.action.impl;
using WinProxyEnvSync.render;
using Install = WinProxyEnvSync.install.Install;

namespace WinProxyEnvSync.notify;

public class Tray
{
  private static readonly MenuRenderer DarkModeRenderer = new(new DarkColorTable(), Color.White);
  private static readonly MenuRenderer LightModeRenderer = new(new LightColorTable(), Color.Black);
  public readonly NotifyIcon _notifyIcon;

  public Tray(NotifyIcon notifyIcon)
  {
    notifyIcon ??= new NotifyIcon
    {
      Icon = LoadAppIcon(),
      Text = "WinProxyEnvSync",
      Visible = true
    };

    _notifyIcon = notifyIcon;
    SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

    notifyIcon.ContextMenuStrip = CreateContextMenu();
    UpdateContextMenuRenderer();

    notifyIcon.DoubleClick += (_, _) => new Status(this).Execute();
  }

  private Icon LoadAppIcon()
  {
    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WinProxyEnvSync.Properties.icon.ico");
    if (stream != null)
    {
      return new Icon(stream);
    }
    return null;
  }

  private ContextMenuStrip CreateContextMenu()
  {
    var contextMenu = new ContextMenuStrip();

    MenuStyler.ApplyStyle(contextMenu);

    var statusMenuItem = new ToolStripMenuItem("Show Status");
    statusMenuItem.Click += (s, e) => new Status(this).Execute();
    contextMenu.Items.Add(statusMenuItem);

    var aboutMenuItem = new ToolStripMenuItem("About");
    aboutMenuItem.Click += (s, e) => new About(this).Execute();
    contextMenu.Items.Add(aboutMenuItem);

    contextMenu.Items.Add(new ToolStripSeparator());

    var exitMenuItem = new ToolStripMenuItem("Exit");
    exitMenuItem.Click += (s, e) => Application.Exit();
    contextMenu.Items.Add(exitMenuItem);

    if (Install.IsInstalled())
    {
      var uninstallMenuItem = new ToolStripMenuItem("Uninstall");
      uninstallMenuItem.Click += (s, e) => new Uninstall(this).Execute();
      contextMenu.Items.Add(uninstallMenuItem);
    }
    else
    {
      var installMenuItem = new ToolStripMenuItem("Install");
      installMenuItem.Click += (s, e) => new action.impl.Install(this).Execute();
      contextMenu.Items.Add(installMenuItem);
    }

    return contextMenu;
  }

  private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
  {
    if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.VisualStyle)
    {
      UpdateContextMenuRenderer();
    }
  }

  public void UpdateContextMenuRenderer()
  {
    if (_notifyIcon?.ContextMenuStrip == null) return;

    _notifyIcon.ContextMenuStrip.Renderer = MenuStyler.IsDarkModeEnabled()
      ? DarkModeRenderer
      : LightModeRenderer;
  }

  public void UpdateContextMenu()
  {
    _notifyIcon.ContextMenuStrip = CreateContextMenu();
    UpdateContextMenuRenderer();

    var oldContextMenu = _notifyIcon.ContextMenuStrip;

    if (oldContextMenu != null)
    {
      oldContextMenu.Dispose();
    }
  }

  public void ShowMessage(string title, string message)
  {
    _notifyIcon.ShowBalloonTip(2000, title, message, ToolTipIcon.None);
  }

  public void Dispose()
  {
    SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
    _notifyIcon.Visible = false;
    _notifyIcon.ContextMenuStrip?.Dispose();
    _notifyIcon.Dispose();
  }
}