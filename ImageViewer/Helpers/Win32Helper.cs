using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using POINT = System.Drawing.Point;

namespace ImageViewer.Helpers
{
    static class Win32Helper
    {
        #region Win32API

        const uint TPM_LEFTBUTTON = 0x0000;
        const uint TPM_RETURNCMD = 0x0100;
        const uint WM_SYSCOMMAND = 0x0112;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT point);

        [DllImport("USER32.dll")]
        public static extern void SetCursorPos(int x, int y);

        [DllImport("USER32.dll")]
        public static extern void ShowCursor(bool isShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern uint TrackPopupMenuEx(IntPtr hMenu, uint fuFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion Win32API

        public static void ShowContextMenu(POINT point)
        {
            var wih = new WindowInteropHelper(Application.Current.MainWindow);
            var myHWnd = wih.Handle;

            var wMenu = GetSystemMenu(myHWnd, false);
            var cmd = TrackPopupMenuEx(wMenu, TPM_LEFTBUTTON | TPM_RETURNCMD, point.X, point.Y, myHWnd, IntPtr.Zero);
            if (cmd != 0)
            {
                PostMessage(myHWnd, WM_SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
            }
        }
    }
}
