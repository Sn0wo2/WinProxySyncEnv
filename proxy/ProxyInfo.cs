using System;
using WinProxyEnvSync.utils;

namespace WinProxyEnvSync.proxy;

public class ProxyInfo(bool proxyEnable, string proxyServer, string proxyOverride)
{
  private const uint WM_SETTING_CHANGE = 0x001A;

  private static readonly IntPtr HWND_BROADCAST = new(0xffff);

  public bool ProxyEnable { get; } = proxyEnable;
  public string ProxyServer { get; } = proxyServer;
  public string ProxyOverride { get; } = proxyOverride;

  public string GetProxy()
  {
    var text = $"Proxy: {ProxyServer ?? "N/A"}";

    if (text.Length > 63) text = text.Substring(0, 60) + "...";
    return text;
  }

  public void SetEnvironmentVariables()
  {
    var proxyServer = ProxyEnable ? $"http://{ProxyServer}" : null;

    Environment.SetEnvironmentVariable("ALL_PROXY", proxyServer, EnvironmentVariableTarget.User);
    Environment.SetEnvironmentVariable("HTTP_PROXY", proxyServer, EnvironmentVariableTarget.User);
    Environment.SetEnvironmentVariable("HTTPS_PROXY", proxyServer, EnvironmentVariableTarget.User);
    Environment.SetEnvironmentVariable("FTP_PROXY", proxyServer, EnvironmentVariableTarget.User);
    Environment.SetEnvironmentVariable("PROXY_URL", proxyServer, EnvironmentVariableTarget.User);

    var proxyOverride = ProxyEnable ? ProxyOverride : null;
    Environment.SetEnvironmentVariable("NO_PROXY", proxyOverride, EnvironmentVariableTarget.User);
    Environment.SetEnvironmentVariable("PROXY_NOC", proxyOverride, EnvironmentVariableTarget.User);

    ProxyUtils.SendMessageTimeout(HWND_BROADCAST, WM_SETTING_CHANGE, UIntPtr.Zero, "Environment",
      ProxyUtils.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out _);
  }

  public override bool Equals(object obj)
  {
    if (obj is ProxyInfo other)
      return ProxyEnable == other.ProxyEnable &&
             ProxyServer == other.ProxyServer &&
             ProxyOverride == other.ProxyOverride;

    return false;
  }

  public override int GetHashCode()
  {
    unchecked
    {
      var hashCode = 17;
      hashCode = hashCode * 23 + ProxyEnable.GetHashCode();
      hashCode = hashCode * 23 + (ProxyServer?.GetHashCode() ?? 0);
      hashCode = hashCode * 23 + (ProxyOverride?.GetHashCode() ?? 0);
      return hashCode;
    }
  }
}