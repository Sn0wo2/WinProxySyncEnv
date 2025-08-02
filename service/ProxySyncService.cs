using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using WinProxyEnvSync.proxy;
using WinProxyEnvSync.utils;
using Timer = System.Timers.Timer;

namespace WinProxyEnvSync.service;

public class ProxySyncService : IDisposable
{
    private readonly object _lock = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly Timer _timer;
    private ProxyInfo _lastProxyInfo;

    public ProxySyncService()
    {
        _timer = new Timer(100) { AutoReset = true };
        _timer.Elapsed += OnTimerElapsed;
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Visible = true,
            Text = "WinProxyEnvSync"
        };
        _notifyIcon.ContextMenu = new ContextMenu([
            new MenuItem("Exit", (s, ev) => Application.Exit())
        ]);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        if (_notifyIcon == null) return;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }


    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (!Monitor.TryEnter(_lock)) return;
        try
        {
            var currentProxyInfo = ProxyUtils.GetCurrentInfo();

            _notifyIcon.Text = $"WinProxyEnvSync\n{currentProxyInfo.GetProxy()}";

            if (currentProxyInfo.Equals(_lastProxyInfo)) return;

            var sb = new StringBuilder();
            if (_lastProxyInfo == null || _lastProxyInfo.ProxyEnable != currentProxyInfo.ProxyEnable)
                sb.AppendLine($"Proxy Enabled: {currentProxyInfo.ProxyEnable}");
            if (_lastProxyInfo == null || _lastProxyInfo.ProxyServer != currentProxyInfo.ProxyServer)
                sb.AppendLine($"Proxy Server: {currentProxyInfo.ProxyServer ?? "None"}");
            if (_lastProxyInfo == null || _lastProxyInfo.ProxyOverride != currentProxyInfo.ProxyOverride)
                sb.AppendLine($"Proxy Override: {currentProxyInfo.ProxyOverride ?? "None"}");
            var changes = sb.ToString().Trim();

            if (string.IsNullOrEmpty(changes)) return;

            Console.WriteLine(changes);
            Console.WriteLine("========================");
            _notifyIcon.ShowBalloonTip(1000, "Proxy Settings Changed", changes, ToolTipIcon.Info);
            currentProxyInfo.SetEnvironmentVariables();
            _lastProxyInfo = currentProxyInfo;
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }

    public void Start()
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================");
        sb.AppendLine("WinProxyEnvSync Started...");
        sb.Append("========================");
        Console.WriteLine(sb.ToString());

        _lastProxyInfo = ProxyUtils.GetCurrentInfo();
        _lastProxyInfo.SetEnvironmentVariables();
        _timer.Start();
        Application.Run();
    }

    public void Stop()
    {
        _timer.Stop();
        Application.Exit();
    }
}