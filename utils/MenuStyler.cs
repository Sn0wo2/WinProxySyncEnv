using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinProxyEnvSync.utils;

public static class MenuStyler
{
  public static void ApplyStyle(ContextMenuStrip menu)
  {
    if (menu == null || menu.IsDisposed || !IsWindows11OrGreater())
    {
      return;
    }

    menu.Opening += (_, _) =>
    {
      try
      {
        var handle = menu.Handle;
        if (handle == IntPtr.Zero) return;

        NativeMethods.SetWindowTheme(handle, "explorer", null);

        var useDarkMode = IsDarkModeEnabled() ? 1 : 0;
        NativeMethods.DwmSetWindowAttribute(handle, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));

        var cornerPreference = (int) NativeMethods.DwmWindowCornerPreference.DWMWCP_ROUND;
        NativeMethods.DwmSetWindowAttribute(handle, NativeMethods.DWM_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to apply menu style: {ex.Message}");
      }
    };
  }

  public static bool IsDarkModeEnabled()
  {
    try
    {
      using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
      if (key == null) return false;

      var value = key.GetValue("AppsUseLightTheme");
      return value is int intValue && intValue == 0;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error checking dark mode: {ex.Message}");
      return false;
    }
  }

  private static bool IsWindows11OrGreater()
  {
    var osVersion = Environment.OSVersion.Version;
    return osVersion.Major >= 10 && osVersion.Build >= 22000;
  }

  private static class NativeMethods
  {
    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    internal const int DWM_WINDOW_CORNER_PREFERENCE = 33;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    internal static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

    internal enum DwmWindowCornerPreference
    {
      DWMWCP_DEFAULT = 0,
      DWMWCP_DONOTROUND = 1,
      DWMWCP_ROUND = 2,
      DWMWCP_ROUNDSMALL = 3
    }
  }
}