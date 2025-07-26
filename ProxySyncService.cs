using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

namespace WinProxyEnvSync
{
    public class ProxySyncService : IDisposable
    {
        private const uint WmSettingchange = 0x001A;

        private static readonly IntPtr HwndBroadcast = new IntPtr(0xffff);
        private readonly object _lock = new object();
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
            _notifyIcon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Exit", (s, ev) => Application.Exit())
            });
        }

        public void Dispose()
        {
            _timer?.Dispose();
            if (_notifyIcon == null) return;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            string lParam,
            SendMessageTimeoutFlags fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult);

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(_lock)) return;

            try
            {
                var currentProxyInfo = GetCurrentProxyInfo();
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
                _notifyIcon.ShowBalloonTip(3000, "Proxy Settings Changed", changes, ToolTipIcon.Info);
                SetEnvironmentVariables(currentProxyInfo);
                _lastProxyInfo = currentProxyInfo;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        private static ProxyInfo GetCurrentProxyInfo()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(
                       @"Software\Microsoft\Windows\CurrentVersion\Internet Settings"))
            {
                if (key == null) return new ProxyInfo(false, null, null);
                var proxyEnable = (int)key.GetValue("ProxyEnable", 0) == 1;
                var proxyServer = proxyEnable ? key.GetValue("ProxyServer") as string : null;
                var proxyOverride = proxyEnable ? key.GetValue("ProxyOverride") as string : null;
                return new ProxyInfo(proxyEnable, proxyServer, proxyOverride);
            }
        }

        private static void SetEnvironmentVariables(ProxyInfo info)
        {
            var proxyServer = info.ProxyEnable ? info.ProxyServer : null;
            var proxyOverride = info.ProxyEnable ? info.ProxyOverride : null;

            Environment.SetEnvironmentVariable("ALL_PROXY", proxyServer, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("HTTP_PROXY", proxyServer, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("HTTPS_PROXY", proxyServer, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("NO_PROXY", proxyOverride, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("FTP_PROXY", proxyServer, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("PROXY_URL", proxyServer != null ? $"http://{proxyServer}" : null,
                EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("PROXY_NOC", proxyOverride, EnvironmentVariableTarget.User);

            SendMessageTimeout(HwndBroadcast, WmSettingchange, UIntPtr.Zero, "Environment",
                SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 5000, out _);
        }

        public void Start()
        {
            _lastProxyInfo = GetCurrentProxyInfo();
            SetEnvironmentVariables(_lastProxyInfo);
            _timer.Start();
            Application.Run();
        }

        public void Stop()
        {
            _timer.Stop();
            Application.Exit();
        }

        private enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }
    }
}