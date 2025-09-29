using System.Windows.Forms;
using WinProxyEnvSync.notify;

namespace WinProxyEnvSync.action.impl;

public class Install(Tray tray) : ITrayAction
{
  public void Execute()
  {
    var result = MessageBox.Show("Do you want to install WinProxyEnvSync? This will add it to startup.", "Confirm Install", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    if (result == DialogResult.Yes)
    {
      install.Install.AddToStartup();
      tray.ShowMessage(null, "WinProxyEnvSync has been installed to startup.");
      tray.UpdateContextMenu();
    }
  }
}