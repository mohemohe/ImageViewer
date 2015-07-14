using System.Diagnostics;
using System.Windows;

namespace ImageViewer
{
    /// <summary>
    ///     App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                return;
            }

            string uri;
            if (UriResolver.IsImageUri(e.Args[0], out uri))
            {
                var window = new MainWindow(uri);
                window.ShowDialog();
            }
            else
            {
                Process.Start(e.Args[0]);
            }
        }
    }
}