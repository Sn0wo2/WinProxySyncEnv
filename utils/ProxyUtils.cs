using System;
using System.Runtime.InteropServices;

namespace WinProxyEnvSync.utils
{
    public static class ProxyUtils
    {
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            string lParam,
            SendMessageTimeoutFlags fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult);
    }
}