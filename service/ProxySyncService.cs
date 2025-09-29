using System;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using WinProxyEnvSync.notify;
using WinProxyEnvSync.proxy;
using WinProxyEnvSync.utils;
using Timer = System.Timers.Timer;

namespace WinProxyEnvSync.service;

public class ProxySyncService : IDisposable
{
  private readonly object _lock = new();
  private readonly Timer _timer;
  private readonly Tray _tray;
  private ProxyInfo _lastProxyInfo;

  public ProxySyncService()
  {
    _tray = new Tray(null);
    _timer = new Timer(2000)
    {
      AutoReset = true
    };
    _timer.Elapsed += OnTimerElapsed;
  }

  public void Dispose()
  {
    new ProxyInfo(false, null, null).SetEnvironmentVariables();
    _timer?.Stop();
    _timer?.Dispose();
    _tray.Dispose();
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

      _tray.NotifyIcon.Text = newText;
      if (currentProxyInfo.Equals(_lastProxyInfo)) return;

      Console.WriteLine("=== Detected proxy settings change ===");

      var changes = BuildChangeMessage(currentProxyInfo);
      Console.WriteLine(changes);

      _tray.ShowMessage(null, changes);

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
}