using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ImageViewer.Helpers
{
    static class Win32Helper
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
