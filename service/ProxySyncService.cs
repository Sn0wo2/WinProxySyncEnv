using System;
using System.Drawing;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;
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
    GCSettings.LatencyMode = GCLatencyMode.Batch;

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

      try
      {
        var newText = $"WinProxyEnvSync\n{currentProxyInfo.GetProxy()}";
        if (newText.Length > 63) newText = newText.Substring(0, 60) + "...";

        if (_notifyIcon != null && _notifyIcon.Text != newText)
          _notifyIcon.Text = newText;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error updating tray icon text: {ex.Message}");
      }

      if (currentProxyInfo.Equals(_lastProxyInfo)) return;

      var changes = BuildChangeMessage(currentProxyInfo);
      if (string.IsNullOrEmpty(changes)) return;

      Console.WriteLine(changes);

      try
      {
        if (_notifyIcon != null)
          _notifyIcon.ShowBalloonTip(1000, "Proxy Settings Changed", changes, ToolTipIcon.Info);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error showing balloon tip: {ex.Message}");
      }

      try
      {
        currentProxyInfo.SetEnvironmentVariables();
        _lastProxyInfo = currentProxyInfo;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error setting environment variables: {ex.Message}");
      }
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

    try
    {
      _lastProxyInfo = ProxyUtils.GetCurrentInfo();
      _lastProxyInfo?.SetEnvironmentVariables();
      _timer.Start();
      Application.Run();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error starting service: {ex.Message}");
      throw;
    }
  }

  public void Stop()
  {
    _timer.Stop();
    Application.Exit();
  }


  private Icon LoadAppIcon()
  {
    try
    {
      var assembly = Assembly.GetExecutingAssembly();
      var resourceName = "WinProxyEnvSync.Properties.icon.ico";

      using var stream = assembly.GetManifestResourceStream(resourceName);
      if (stream != null)
      {
        return new Icon(stream);
      }
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine(ex);
    }

    return null;
  }

  private void ShowMessage(string message)
  {
    _notifyIcon.ShowBalloonTip(2000, "WinProxyEnvSync", message, ToolTipIcon.Info);
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

    return contextMenu;
  }

  private void ShowCurrentStatus()
  {
    try
    {
      var currentProxy = ProxyUtils.GetCurrentInfo();
      if (currentProxy == null)
      {
        _notifyIcon?.ShowBalloonTip(2000, "Status", "Unable to get proxy status", ToolTipIcon.Warning);
        return;
      }

      var statusText = $"Proxy Status:\nEnabled: {(currentProxy.ProxyEnable ? "Yes" : "No")}\nServer: {currentProxy.ProxyServer ?? "None"}\nOverride: {currentProxy.ProxyOverride ?? "None"}";
      ShowMessage(statusText);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error showing current status: {ex.Message}");
      try
      {
        _notifyIcon?.ShowBalloonTip(2000, "Error", "Failed to get proxy status", ToolTipIcon.Error);
      }
      catch
      {
        // Ignore exceptions when showing error balloon tip
      }
    }
  }
}