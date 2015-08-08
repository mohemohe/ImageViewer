using System;
using System.Runtime.InteropServices;

namespace ImageViewer.Helpers
{
    static class Win32Helper
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out System.Drawing.Point point);

        [DllImport("USER32.dll")]
        public static extern void SetCursorPos(int x, int y);

        [DllImport("USER32.dll")]
        public static extern void ShowCursor(bool isShow);
    }
}
