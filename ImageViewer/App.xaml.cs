using ImageViewer.Helpers;
using ImageViewer.Models;
using ImageViewer.Views;
using Livet;
using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using ImageViewer.Infrastructures;

namespace ImageViewer
{
    /// <summary>
    ///     App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = new Mutex(false, Application.ResourceAssembly.GetName().Name);

        public App() : base()
        {
            QuickConverter.EquationTokenizer.AddAssembly(typeof(object).Assembly);
            QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
            QuickConverter.EquationTokenizer.AddNamespace(typeof(string));
            QuickConverter.EquationTokenizer.AddNamespace(typeof(System.Windows.Media.SolidColorBrush));
            QuickConverter.EquationTokenizer.AddNamespace(typeof(System.Windows.Media.Colors));
        }

#if !DEBUG

        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                Environment.Exit(0);
            }

            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
            Config.ReadConfig();
            this.Exit += (s, a) =>
            {
                Config.WriteConfig();
            };

            if (Config.IsEnablePseudoSingleInstance)
            {
                if (mutex.WaitOne(0, false) == false)
                {
                    IpcClientChannel ipc = new IpcClientChannel();
                    ChannelServices.RegisterChannel(ipc, true);
                    Message message = (Message)RemotingServices.Connect(typeof(Message), @"ipc://" + Application.ResourceAssembly.GetName().Name + @"/Message");
                    message.RaiseHandler(e.Args);

                    mutex.Close();
                    mutex = null;

                    // Note: うまく終了しないことがある
                    //this.Shutdown();
                    Environment.Exit(0);
                }
            }

            string uri = e.Args[0];
            string imageUri;
            if (UriRouter.IsImageUri(ref uri, out imageUri))
            {
                var window = new MainWindow();

                if (Config.IsChildWindow)
                {
                    var wih = new WindowInteropHelper(window);
                    wih.Owner = Win32Helper.GetForegroundWindow();
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Top = Config.WindowPosition.Top;
                    window.Left = Config.WindowPosition.Left;
                }

                if (Config.WindowPosition.Width != 0)
                {
                    window.Width = Config.WindowPosition.Width;
                    window.Height = Config.WindowPosition.Height;
                }
                window.Show();

                window.VM.AddTab(imageUri, uri);

                if (Config.IsEnablePseudoSingleInstance)
                {
                    IpcServerChannel ipc = new IpcServerChannel(Application.ResourceAssembly.GetName().Name);
                    ChannelServices.RegisterChannel(ipc, true);
                    Message message = new Message();
                    message.MessageHandler += ((string[] args) =>
                    {
                        if (UriRouter.IsImageUri(ref args[0], out imageUri))
                        {
                            window.VM.AddTab(imageUri, args[0]);
                        }
                    });
                    RemotingServices.Marshal(message, "Message");
                }
            }
            else
            {
                if (Config.DefaultBrowserPath == null)
                {
                    Process.Start(uri);
                }
                else
                {
                    var psi = new ProcessStartInfo { Arguments = uri, FileName = Config.DefaultBrowserPath };
                    Process.Start(psi);
                }

                Environment.Exit(0);
            }
        }

#endif
#if DEBUG
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Config.ReadConfig();
            this.Exit += (s, a) =>
            {
                Config.WriteConfig();
            };

            string testUri = "http://s.kuku.lu/23l3e6768";

            if (Config.IsEnablePseudoSingleInstance)
            {
                if (mutex.WaitOne(0, false) == false)
                {
                    var ipc = new IpcClientChannel();
                    ChannelServices.RegisterChannel(ipc, true);
                    var message = (Message)RemotingServices.Connect(typeof(Message), @"ipc://" + Application.ResourceAssembly.GetName().Name + @"/Message");
                    message.RaiseHandler(new string[] { testUri });

                    mutex.Close();
                    mutex = null;
                    this.Shutdown();
                }
            }

            string imageUri;
            if (UriRouter.IsImageUri(ref testUri, out imageUri))
            {
                var window = new MainWindow();

                if (Config.IsChildWindow)
                {
                    var wih = new WindowInteropHelper(window);
                    wih.Owner = Win32Helper.GetForegroundWindow();
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Top = Config.WindowPosition.Top;
                    window.Left = Config.WindowPosition.Left;
                }

                if (Config.WindowPosition.Width != 0)
                {
                    window.Width = Config.WindowPosition.Width;
                    window.Height = Config.WindowPosition.Height;
                }
                window.Show();

                window.VM.AddTab(imageUri, testUri);

                if (Config.IsEnablePseudoSingleInstance)
                {
                    var ipc = new IpcServerChannel(Application.ResourceAssembly.GetName().Name);
                    ChannelServices.RegisterChannel(ipc, true);
                    var message = new Message();
                    message.MessageHandler += ((string[] args) =>
                    {
                        if (UriRouter.IsImageUri(ref args[0], out imageUri))
                        {
                            window.VM.AddTab(imageUri, args[0]);
                        }
                    });
                    RemotingServices.Marshal(message, "Message");
                }
            }
        }
#endif

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var window = new ExceptionWindow(e);
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // true: 続行, false: 終了
            var result = window.ShowDialog();
            if (result != null)
            {
                if ((bool)result)
                {
                    Process.Start(Application.ResourceAssembly.Location);
                }

                // 遅い
                //Environment.Exit(1);

                // 終了コード-1を返したい
                Process.GetCurrentProcess().Kill();
            }
        }
    }

    delegate void MessageHandler(string[] args);

    class Message : MarshalByRefObject
    {
        public MessageHandler MessageHandler;

        public void RaiseHandler(string[] args)
        {
            DispatcherHelper.UIDispatcher.BeginInvoke(MessageHandler, new object[] { args });
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}