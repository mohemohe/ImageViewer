using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageViewer.Views.MainWindow
{
    public class WindowBase : Window
    {
        static WindowBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
            BackgroundProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(SystemColors.WindowBrush));
        }
    }
}
