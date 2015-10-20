using System;
using System.Collections.Generic;
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

        [Flags]
        public enum MenuFlags : uint
        {
            STRING = 0,
            SEPARATOR = 0x800,
        }

        const uint TPM_LEFTBUTTON = 0x0000;
        const uint TPM_RETURNCMD = 0x0100;
        const uint WM_SYSCOMMAND = 0x0112;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT point);

        [DllImport("user32.dll")]
        public static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void ShowCursor(bool isShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint PrivateExtractIcons(string lpszFile, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, IntPtr[] piconid, uint nIcons, uint flags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool AppendMenu(IntPtr hMenu, MenuFlags uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        public static extern uint TrackPopupMenuEx(IntPtr hMenu, uint fuFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

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

        private static Dictionary<uint, Action> _menuActionDictionary = new Dictionary<uint, Action>();
        private static HwndSourceHook _hook;

        public static void MainWindowAppendMenu(MenuFlags menuFlag, string menuTitle, Action action)
        {
            var wih = new WindowInteropHelper(Application.Current.MainWindow);
            var myHWnd = wih.Handle;

            var hMenu = GetSystemMenu(myHWnd, false);

            uint uIDNewItem = (uint)_menuActionDictionary.Count + 1001;
            _menuActionDictionary.Add(uIDNewItem, action);

            AppendMenu(hMenu, menuFlag, uIDNewItem, menuTitle);

            var source = HwndSource.FromHwnd(myHWnd);
            // NOTE: hookしなおす必要があるのか分からん
            if (_hook != null)
            {
                source.RemoveHook(_hook);
            }

            _hook = new HwndSourceHook((IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                if (msg == WM_SYSCOMMAND && _menuActionDictionary.ContainsKey((uint)wParam))
                {
                    _menuActionDictionary[(uint)wParam]();
                    handled = true;
                }
                return IntPtr.Zero;
            });
            source.AddHook(_hook);
        }

        public static Icon GetIcon(string path, int index = 0, int size = 128)
        {
            var phIcon = new IntPtr[] { IntPtr.Zero };
            var pIconId = new IntPtr[] { IntPtr.Zero };

            PrivateExtractIcons(path, index, size, size, phIcon, pIconId, 1, 0);

            return phIcon[0] != IntPtr.Zero ? Icon.FromHandle(phIcon[0]) : null;
        }
    }
}
