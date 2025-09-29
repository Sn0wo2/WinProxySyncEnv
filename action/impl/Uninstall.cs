using System.Windows.Forms;
using WinProxyEnvSync.notify;

namespace WinProxyEnvSync.action.impl;

public class Uninstall(Tray tray) : ITrayAction
{
  public void Execute()
  {
    var result = MessageBox.Show("Are you sure you want to uninstall WinProxyEnvSync? This will remove it from startup.", "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    if (result == DialogResult.Yes)
    {
      install.Install.RemoveFromStartup();
      tray.ShowMessage(null, "WinProxyEnvSync has been uninstalled from startup.");
      tray.UpdateContextMenu();
    }
  }
}