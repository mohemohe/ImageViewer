using ImageViewer.Models;
using ImageViewer.Views;
using Livet;
using System;
using System.Diagnostics;
using System.Windows;

namespace ImageViewer
{
    /// <summary>
    ///     App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
#if !DEBUG

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                Environment.Exit(0);
            }

            DispatcherHelper.UIDispatcher = Dispatcher;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            string imageUri;
            if (UriRouter.IsImageUri(e.Args[0], out imageUri))
            {
                var window = new MainWindow(imageUri);

                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = 0;
                window.Top = 0;
                window.Show();
            }
            else
            {
                Process.Start(e.Args[0]);
                Environment.Exit(0);
            }
        }

#endif
#if DEBUG
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            string testUri = "http://p.twipple.jp/wnpwe";
            string imageUri;
            if (UriRouter.IsImageUri(testUri, out imageUri))
            {
                var window = new MainWindow(imageUri);

                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = 0;
                window.Top = 0;
                window.Show();
            }
        }
#endif

        //集約エラーハンドラ
        //private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    //TODO:ロギング処理など
        //    MessageBox.Show(
        //        "不明なエラーが発生しました。アプリケーションを終了します。",
        //        "エラー",
        //        MessageBoxButton.OK,
        //        MessageBoxImage.Error);
        //
        //    Environment.Exit(1);
        //}
    }
}