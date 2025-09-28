using System;
using System.Reflection;
using Microsoft.Win32;

namespace WinProxyEnvSync.install;

public class Install
{
  private const string StartupKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
  private static readonly string AppPath = Assembly.GetExecutingAssembly().Location;

  public static void AddToStartup()
  {
    using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, true);
    if (key == null)
    {
      Console.WriteLine("Error: Could not open startup registry key.");
      return;
    }

    key.SetValue(Program.AppName, $"\"{AppPath}\"");
    Console.WriteLine("Application added to startup successfully.");
  }

  public static void RemoveFromStartup()
  {
    using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, true);
    if (key?.GetValue(Program.AppName) != null)
    {
      key.DeleteValue(Program.AppName, false);
      Console.WriteLine("Application removed from startup successfully.");
    }
    else
    {
      Console.WriteLine("Application not found in startup.");
    }
  }

  public static bool IsInstalled()
  {
    return Registry.CurrentUser.OpenSubKey(StartupKeyPath, false)?.GetValue(Program.AppName) != null;
  }
}