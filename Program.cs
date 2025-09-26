using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using WinProxyEnvSync.service;

namespace WinProxyEnvSync;

internal static class Program
{
  private const string AppName = "WinProxyEnvSync";
  private const string StartupKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
  private static readonly string AppPath = Assembly.GetExecutingAssembly().Location;

  [STAThread]
  public static void Main(string[] args)
  {
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
          AddToStartup();
          break;
        case "uninstall":
          RemoveFromStartup();
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

  private static void AddToStartup()
  {
    try
    {
      using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, true);
      if (key == null)
      {
        Console.WriteLine("Error: Could not open startup registry key.");
        return;
      }

      key.SetValue(AppName, $"\"{AppPath}\"");
      Console.WriteLine("Application added to startup successfully.");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error adding to startup: {ex.Message}");
    }
  }

  private static void RemoveFromStartup()
  {
    try
    {
      using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, true);
      if (key?.GetValue(AppName) != null)
      {
        key.DeleteValue(AppName, false);
        Console.WriteLine("Application removed from startup successfully.");
      }
      else
      {
        Console.WriteLine("Application not found in startup.");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error removing from startup: {ex.Message}");
    }
  }

  private static void RunApp()
  {
    using var mutex = new Mutex(true, AppName, out var createdNew);
    if (!createdNew)
    {
      return;
    }

    using var service = new ProxySyncService();
    service.Start();
  }
}