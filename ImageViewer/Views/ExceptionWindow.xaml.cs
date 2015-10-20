using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Interop;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Livet;
using ImageViewer.Helpers;

namespace ImageViewer.Views
{
    /* 
	 * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// ExceptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        

        public ExceptionWindow(UnhandledExceptionEventArgs e)
        {
            InitializeComponent();

            SetErrorIcon();

            var exception = (Exception)e.ExceptionObject;
            ExceptionTitle.Text = exception.GetType().ToString();
            Message.Text = exception.Message;
            StackTrace.Text = exception.StackTrace;
        }

        private void SetErrorIcon()
        {
            var icon = Win32Helper.GetIcon(@"user32.dll", 3).ToBitmap();
            var hBitmap = icon.GetHbitmap();
            BitmapSource source = null;

            try
            {
                source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                Win32Helper.DeleteObject(hBitmap);
            }

            Icon.Source = source;
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}