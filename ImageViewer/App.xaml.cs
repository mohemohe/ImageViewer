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
using System.Windows.Threading;

namespace ImageViewer
{
    /// <summary>
    ///     App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = new Mutex(false, Application.ResourceAssembly.GetName().Name);

#if !DEBUG

        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                Environment.Exit(0);
            }

            DispatcherHelper.UIDispatcher = Dispatcher;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (mutex.WaitOne(0, false) == false)
            {
                IpcClientChannel ipc = new IpcClientChannel();
                ChannelServices.RegisterChannel(ipc, true);
                Message message = (Message)RemotingServices.Connect(typeof(Message), @"ipc://" + Application.ResourceAssembly.GetName().Name + @"/Message");
                message.RaiseHandler(e.Args);

                mutex.Close();
                mutex = null;
                this.Shutdown();
            }

            string imageUri;
            if (UriRouter.IsImageUri(e.Args[0], out imageUri))
            {
                var window = new MainWindow();
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = 0;
                window.Top = 0;
                window.Show();

                window.VM.AddTab(imageUri, e.Args[0]);

                IpcServerChannel ipc = new IpcServerChannel(Application.ResourceAssembly.GetName().Name);
                ChannelServices.RegisterChannel(ipc, true);
                Message message = new Message();
                message.MessageHandler += ((string[] args) =>
                {
                    if (UriRouter.IsImageUri(args[0], out imageUri))
                    {
                        window.VM.AddTab(imageUri, args[0]);
                    }
                });
                RemotingServices.Marshal(message, "Message");
            }
            else
            {
                Process.Start(e.Args[0]);
                Environment.Exit(0);
            }
        }

#endif
#if DEBUG
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if(mutex.WaitOne(0, false) == false)
            {
                IpcClientChannel ipc = new IpcClientChannel();
                ChannelServices.RegisterChannel(ipc, true);
                Message message = (Message)RemotingServices.Connect(typeof(Message), @"ipc://" + Application.ResourceAssembly.GetName().Name + @"/Message");
                message.RaiseHandler(new string[] { "http://s.kuku.lu/70i95am9k" });

                mutex.Close();
                mutex = null;
                this.Shutdown();
            }

            string testUri = "http://s.kuku.lu/70i95am9k";
            string imageUri;
            if (UriRouter.IsImageUri(testUri, out imageUri))
            {
                var window = new MainWindow();

                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = 0;
                window.Top = 0;
                window.Show();

                window.VM.AddTab(imageUri, testUri);

                IpcServerChannel ipc = new IpcServerChannel(Application.ResourceAssembly.GetName().Name);
                ChannelServices.RegisterChannel(ipc, true);
                Message message = new Message();
                message.MessageHandler += ((string[] args) =>
                {
                    if (UriRouter.IsImageUri(args[0], out imageUri))
                    {
                        window.VM.AddTab(imageUri, args[0]);
                    }
                });
                RemotingServices.Marshal(message, "Message");
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