using System;
using WinProxyEnvSync.notify;

namespace WinProxyEnvSync.action.impl;

public class Install(Tray _) : ITrayAction
{
  public Tray Tray { get; } = _;

  public void Execute()
  {
    throw new NotImplementedException();
  }
}