using System;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace WinProxyEnvSync
{
    internal static class Program
    {
        private const string AppName = "WinProxyEnvSync";
        private static readonly string StartupKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string AppPath = Assembly.GetExecutingAssembly().Location;

        public static void Main(string[] args)
        {
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

        private static void AddToStartup()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, true))
                {
                    if (key == null)
                    {
                        Console.WriteLine("Error: Could not open startup registry key.");
                        return;
                    }

                    key.SetValue(AppName, $"\"{AppPath}\"");
                    Console.WriteLine("Application added to startup successfully.");
                }
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
                using (var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, true))
                {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from startup: {ex.Message}");
            }
        }

        private static void RunApp()
        {
            using (var proxySyncService = new ProxySyncService())
            {
                proxySyncService.Start();
            }
        }
    }
}