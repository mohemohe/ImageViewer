using ImageViewer.Helpers;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

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
    ///     ExceptionWindow.xaml の相互作用ロジック
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow(UnhandledExceptionEventArgs e)
        {
            InitializeComponent();

            SetErrorIcon();

            var exception = (Exception) e.ExceptionObject;
            ExceptionTitle.Text = exception.GetType().ToString();
            Message.Text = exception.Message;
            StackTrace.Text = exception.StackTrace;
        }

        private void SetErrorIcon()
        {
            var icon = Win32Helper.GetIcon(@"user32.dll", 3).ToBitmap();
            var hBitmap = icon.GetHbitmap();
            BitmapSource source;

            try
            {
                source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                Win32Helper.DeleteObject(hBitmap);
            }

            ErrorIcon.Source = source;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}