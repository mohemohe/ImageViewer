using ImageViewer.Helpers;
using ImageViewer.Infrastructures;
using ImageViewer.Models;
using ImageViewer.Views;
using Livet;
using QuickConverter;
using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ImageViewer
{
    /// <summary>
    ///     App.xaml の相互作用ロジック
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private MainWindow _mainWindow;
        private Mutex _mutex = new Mutex(false, ResourceAssembly.GetName().Name);

        public App()
        {
            EquationTokenizer.AddAssembly(typeof (object).Assembly);
            EquationTokenizer.AddNamespace(typeof (object));
            EquationTokenizer.AddNamespace(typeof (string));
            EquationTokenizer.AddNamespace(typeof (SolidColorBrush));
            EquationTokenizer.AddNamespace(typeof (Colors));
        }

        ~App()
        {
            _mutex.Close();
            _mutex.Dispose();
            _mutex = null;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Config.ReadConfig();

#if !DEBUG
            if (e.Args.Length == 0)
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
                Config.WriteConfig();
                Environment.Exit(0);
            }
#endif
            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            
            Exit += (s, a) => { Config.WriteConfig(); };

            if (Config.IsEnablePseudoSingleInstance && _mutex.WaitOne(0, false) == false)
            {
                var ipc = new IpcClientChannel();
                ChannelServices.RegisterChannel(ipc, true);
                var message =
                    (Message)
                        RemotingServices.Connect(typeof (Message),
                            @"ipc://" + ResourceAssembly.GetName().Name + @"/Message");
                message.RaiseHandler(e.Args);

                // Note: うまく終了しないことがある
                //this.Shutdown();
                Environment.Exit(0);
            }

            var openInBrowser = new Func<string, int>(a =>
            {
                if (Config.DefaultBrowserPath == null)
                {
                    Process.Start(a);
                }
                else
                {
                    var psi = new ProcessStartInfo {Arguments = a, FileName = Config.DefaultBrowserPath};
                    Process.Start(psi);
                }

                return 0;
            });

#if !DEBUG
            var uri = e.Args[0];
#else
            var uri = "http://pbs.twimg.com/media/CPFBu36VEAA16jI.png:orig";
#endif

            string imageUri;
            if (UriRouter.IsImageUri(ref uri, out imageUri))
            {
                _mainWindow = new MainWindow();

                if (Config.IsChildWindow)
                {
                    var wih = new WindowInteropHelper(_mainWindow);
                    wih.Owner = Win32Helper.GetForegroundWindow();
                    _mainWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    _mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    _mainWindow.Top = Config.WindowPosition.Top;
                    _mainWindow.Left = Config.WindowPosition.Left;
                }

                if ((int) Config.WindowPosition.Width != 0)
                {
                    _mainWindow.Width = Config.WindowPosition.Width;
                    _mainWindow.Height = Config.WindowPosition.Height;
                }
                _mainWindow.Show();

                _mainWindow.VM.AddTab(imageUri, uri);

                if (Config.IsEnablePseudoSingleInstance)
                {
                    var ipc = new IpcServerChannel(ResourceAssembly.GetName().Name);
                    ChannelServices.RegisterChannel(ipc, true);
                    var message = new Message();
                    message.MessageHandler += (args =>
                    {
                        if (UriRouter.IsImageUri(ref args[0], out imageUri))
                        {
                            _mainWindow.VM.AddTab(imageUri, args[0]);
                        }
                        else
                        {
                            openInBrowser(args[0]);
                        }
                    });
                    RemotingServices.Marshal(message, "Message");
                }
            }
            else
            {
                Environment.Exit(openInBrowser(uri));
            }
        }

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var window = new ExceptionWindow(e);
            if (Current.MainWindow?.DataContext != null)
            {
                _mainWindow?.Hide();
                window.Owner = Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            // true: 続行, false: 終了
            var result = window.ShowDialog();
            if (result != null)
            {
                // 遅い
                //Environment.Exit(1);

                // 終了コード-1を返したい
                _mutex.Close();
                _mutex.Dispose();
                _mutex = null;
                Process.GetCurrentProcess().Kill();
            }
        }
    }

    internal delegate void MessageHandler(string[] args);

    internal class Message : MarshalByRefObject
    {
        public MessageHandler MessageHandler;

        public void RaiseHandler(string[] args)
        {
            DispatcherHelper.UIDispatcher.BeginInvoke(MessageHandler, new object[] {args});
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}