using System.Reflection;
using System.Windows.Forms;
using WinProxyEnvSync.notify;

namespace WinProxyEnvSync.action.impl;

public class About(Tray tray) : ITrayAction
{
  public Tray Tray { get; } = tray;

  public void Execute()
  {
    MessageBox.Show($"WinProxyEnvSync v{Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0"}\nSync windows proxy settings to environment variables\n\nhttps://github.com/Sn0wo2/WinProxySyncEnv", "About WinProxyEnvSync", MessageBoxButtons.OK, MessageBoxIcon.Information);
  }
}