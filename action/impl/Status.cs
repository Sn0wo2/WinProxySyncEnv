using WinProxyEnvSync.notify;
using WinProxyEnvSync.utils;

namespace WinProxyEnvSync.action.impl;

public class Status(Tray tray) : ITrayAction
{
  public void Execute()
  {
    var currentProxy = ProxyUtils.GetCurrentInfo();
    tray.ShowMessage(null, $"Proxy Status:\nEnabled: {(currentProxy.ProxyEnable ? "Yes" : "No")}\nServer: {currentProxy.ProxyServer ?? "None"}\nOverride: {currentProxy.ProxyOverride ?? "None"}");
  }
}