using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;
using WinProxyEnvSync.install;
using WinProxyEnvSync.service;

namespace WinProxyEnvSync;

internal static class Program
{
  public const string AppName = "WinProxyEnvSync";

  [STAThread]
  public static void Main(string[] args)
  {
    GCSettings.LatencyMode = GCLatencyMode.Batch;
    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
    Application.ThreadException += ApplicationThreadException;
    AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

    try
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      Process.GetCurrentProcess().PriorityClass =
        ProcessPriorityClass.BelowNormal;

      var command = args.FirstOrDefault()?.ToLowerInvariant();
      switch (command)
      {
        case "install":
        case "i":
          Install.AddToStartup();
          break;
        case "uninstall":
          Install.RemoveFromStartup();
          break;
        default:
          RunApp();
          break;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Fatal error in Main method: {ex}");
      Environment.Exit(-1);
    }
  }

  private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
  {
    Console.WriteLine($"Application thread exception: {e.Exception}");
    MessageBox.Show($"The application encountered an error: {e.Exception.Message}",
      "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
  }

  private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
  {
    var ex = e.ExceptionObject as Exception ?? new Exception($"Unknown exception object: {e.ExceptionObject}");
    Console.WriteLine($"Unhandled application domain exception: {ex}");

    if (e.IsTerminating)
    {
      MessageBox.Show($"The application encountered a fatal error and will now exit: {ex.Message}",
        "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }

  private static void RunApp()
  {
    using var mutex = new Mutex(true, AppName, out var createdNew);
    if (!createdNew)
    {
      Console.WriteLine("Another instance is already running. Exiting.");
      return;
    }

    new ProxySyncService().Start();
  }
}