using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using WinProxyEnvSync.proxy;

namespace WinProxyEnvSync.utils;

public static class ProxyUtils
{
  public enum SendMessageTimeoutFlags : uint
  {
    SMTO_NORMAL = 0x0,
    SMTO_BLOCK = 0x1,
    SMTO_ABORTIFHUNG = 0x2,
    SMTO_NOTIMEOUTIFNOTHUNG = 0x8
  }

  // https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-settingchange
  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  public static extern IntPtr SendMessageTimeout(
    IntPtr hWnd,
    uint Msg,
    UIntPtr wParam,
    string lParam,
    SendMessageTimeoutFlags fuFlags,
    uint uTimeout,
    out UIntPtr lpdwResult);

  public static ProxyInfo GetCurrentInfo()
  {
    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", false);

    if (key?.GetValue("ProxyEnable") is not int proxyEnableValue)
      return new ProxyInfo(false, null, null);

    var proxyEnable = proxyEnableValue == 1;

    var proxyServer = proxyEnable ? key.GetValue("ProxyServer") as string : null;
    var proxyOverride = proxyEnable ? key.GetValue("ProxyOverride") as string : null;

    return new ProxyInfo(proxyEnable, proxyServer, proxyOverride);
  }
}