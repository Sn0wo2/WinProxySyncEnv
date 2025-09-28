using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;
using WinProxyEnvSync.install;
using WinProxyEnvSync.proxy;
using WinProxyEnvSync.utils;
using Timer = System.Timers.Timer;

namespace WinProxyEnvSync.service;

public class ProxySyncService : IDisposable
{
  private static readonly MenuRenderer DarkModeRenderer = new(new DarkColorTable(), Color.White);
  private static readonly MenuRenderer LightModeRenderer = new(new LightColorTable(), Color.Black);
  private readonly Icon _appIcon;
  private readonly object _lock = new();
  private readonly NotifyIcon _notifyIcon;
  private readonly Timer _timer;
  private ProxyInfo _lastProxyInfo;

  public ProxySyncService()
  {
    _timer = new Timer(2000)
    {
      AutoReset = true
    };
    _timer.Elapsed += OnTimerElapsed;

    _appIcon = LoadAppIcon();

    _notifyIcon = new NotifyIcon
    {
      Icon = _appIcon ?? SystemIcons.Information,
      Visible = true,
      Text = "WinProxyEnvSync"
    };

    _notifyIcon.ContextMenuStrip = CreateContextMenu();
    UpdateContextMenuRenderer();

    _notifyIcon.DoubleClick += (s, ev) => ShowCurrentStatus();

    SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
  }

  public void Dispose()
  {
    SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;

    new ProxyInfo(false, null, null).SetEnvironmentVariables();

    try
    {
      _timer?.Stop();
      _timer?.Dispose();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error disposing timer: {ex.Message}");
    }

    try
    {
      if (_notifyIcon != null)
      {
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error disposing notify icon: {ex.Message}");
    }

    try
    {
      _appIcon?.Dispose();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error disposing app icon: {ex.Message}");
    }
  }

  private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
  {
    if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.VisualStyle)
    {
      UpdateContextMenuRenderer();
    }
  }

  private void UpdateContextMenuRenderer()
  {
    if (_notifyIcon?.ContextMenuStrip == null) return;

    _notifyIcon.ContextMenuStrip.Renderer = MenuStyler.IsDarkModeEnabled()
      ? DarkModeRenderer
      : LightModeRenderer;
  }


  private void OnTimerElapsed(object sender, ElapsedEventArgs e)
  {
    if (!Monitor.TryEnter(_lock)) return;
    try
    {
      var currentProxyInfo = ProxyUtils.GetCurrentInfo();
      if (currentProxyInfo == null) return;

      var newText = $"WinProxyEnvSync\n{currentProxyInfo.GetProxy()}";
      if (newText.Length > 63) newText = newText.Substring(0, 60) + "...";

      if (_notifyIcon != null && _notifyIcon.Text != newText)
        _notifyIcon.Text = newText;
      if (currentProxyInfo.Equals(_lastProxyInfo)) return;


      Console.WriteLine("=== Detected proxy settings change ===");

      var changes = BuildChangeMessage(currentProxyInfo);
      Console.WriteLine(changes);

      ShowMessage(null, changes);

      currentProxyInfo.SetEnvironmentVariables();
      _lastProxyInfo = currentProxyInfo;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error in timer event handler: {ex.Message}");
    }
    finally
    {
      Monitor.Exit(_lock);
    }
  }


  private string BuildChangeMessage(ProxyInfo currentProxyInfo)
  {
    var sb = new StringBuilder();
    sb.AppendLine("Proxy settings changed:");
    if (_lastProxyInfo == null || _lastProxyInfo.ProxyEnable != currentProxyInfo.ProxyEnable)
      sb.AppendLine($"Proxy Enabled: {currentProxyInfo.ProxyEnable}");
    if (_lastProxyInfo == null || _lastProxyInfo.ProxyServer != currentProxyInfo.ProxyServer)
      sb.AppendLine($"Proxy Server: {currentProxyInfo.ProxyServer ?? "None"}");
    if (_lastProxyInfo == null || _lastProxyInfo.ProxyOverride != currentProxyInfo.ProxyOverride)
      sb.AppendLine($"Proxy Override: {currentProxyInfo.ProxyOverride ?? "None"}");
    return sb.ToString().Trim();
  }

  public void Start()
  {
    Console.WriteLine("WinProxyEnvSync Started...");

    _timer.Start();
    Application.Run();
  }

  public void Stop()
  {
    _timer.Stop();
    Application.Exit();
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

  private void ShowMessage(string title, string message)
  {
    _notifyIcon.ShowBalloonTip(2000, title, message, ToolTipIcon.None);
  }

  private void ShowAbout()
  {
    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    var message = $"WinProxyEnvSync v{version}\nSync windows proxy settings to environment variables\n\nhttps://github.com/Sn0wo2/WinProxySyncEnv";
    MessageBox.Show(message, "About WinProxyEnvSync", MessageBoxButtons.OK, MessageBoxIcon.Information);
  }


  private ContextMenuStrip CreateContextMenu()
  {
    var contextMenu = new ContextMenuStrip();

    MenuStyler.ApplyStyle(contextMenu);

    var statusMenuItem = new ToolStripMenuItem("Show Status");
    statusMenuItem.Click += (s, e) => ShowCurrentStatus();
    contextMenu.Items.Add(statusMenuItem);

    var aboutMenuItem = new ToolStripMenuItem("About");
    aboutMenuItem.Click += (s, e) => ShowAbout();
    contextMenu.Items.Add(aboutMenuItem);

    contextMenu.Items.Add(new ToolStripSeparator());

    var exitMenuItem = new ToolStripMenuItem("Exit");
    exitMenuItem.Click += (s, e) => Application.Exit();
    contextMenu.Items.Add(exitMenuItem);

    if (Install.IsInstalled())
    {
      var uninstallMenuItem = new ToolStripMenuItem("Uninstall");
      uninstallMenuItem.Click += (s, e) =>
      {
        var result = MessageBox.Show("Are you sure you want to uninstall WinProxyEnvSync? This will remove it from startup.", "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
          Install.RemoveFromStartup();
          ShowMessage(null, "WinProxyEnvSync has been uninstalled from startup.");
          UpdateContextMenu();
        }
      };
      contextMenu.Items.Add(uninstallMenuItem);
    }
    else
    {
      var installMenuItem = new ToolStripMenuItem("Install");
      installMenuItem.Click += (s, e) =>
      {
        var result = MessageBox.Show("Do you want to install WinProxyEnvSync? This will add it to startup.", "Confirm Install", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
          Install.AddToStartup();
          ShowMessage(null, "WinProxyEnvSync has been installed to startup.");
          UpdateContextMenu();
        }
      };
      contextMenu.Items.Add(installMenuItem);
    }

    return contextMenu;
  }

  private void UpdateContextMenu()
  {
    _notifyIcon.ContextMenuStrip = CreateContextMenu();
    UpdateContextMenuRenderer();

    var oldContextMenu = _notifyIcon.ContextMenuStrip;

    if (oldContextMenu != null)
    {
      oldContextMenu.Dispose();
    }
  }

  private void ShowCurrentStatus()
  {
    var currentProxy = ProxyUtils.GetCurrentInfo();
    ShowMessage(null, $"Proxy Status:\nEnabled: {(currentProxy.ProxyEnable ? "Yes" : "No")}\nServer: {currentProxy.ProxyServer ?? "None"}\nOverride: {currentProxy.ProxyOverride ?? "None"}");
  }
}