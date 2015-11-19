using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using ImageViewer.Models;
using ImageViewer.ViewModels;
using ImageViewer.Views.ViewWindow;

namespace HUSauth.Helpers
{
    public static class NotifyIconHelper
    {
        private static NotifyIcon _notifyIcon;
        public static TabWindow Window { get; private set; }
        public static bool IsClosable { get; private set; }

        public static void Initialize(dynamic window)
        {
            Bitmap icon;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(new Uri(@"pack://application:,,,/Resources/ImageViewer.ico", UriKind.Absolute)));
                enc.Save(outStream);
                icon = new Bitmap(outStream);
            }
            
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.FromHandle(icon.GetHicon()),
                Visible = true
            };

            var cms = new ContextMenuStrip();
            var tsm = new ToolStripMenuItem {Text = @"Exit"};
            tsm.Click += (sender, e) =>
            {
                Exit();
            };
            cms.Items.Add(tsm);
            _notifyIcon.ContextMenuStrip = cms;

            Window = window;
        }

        public static void TryShow()
        {
            Window?.Show();
        }

        public static void Close()
        {
            Window.Hide();
            Window.VM.DeferredImageItems = new ObservableCollection<ImageItem>();
            Window.ForceFullGC();

            ShowNotifyBaloon(
                @"Aggressive Mode",
                @"You have to right click the icon and select Exit in order to truly close it.");
        }

        public static void Exit()
        {
            IsClosable = true;

            _notifyIcon.Dispose();
            Window?.Close();
            Config.WriteConfig();
            Environment.Exit(0);
        }

        public static void TryDispose()
        {
            _notifyIcon?.Dispose();
        }

        #region public static void ShowNotifyBaloon()

        public static void ShowNotifyBaloon(string title, string body, int timeout = 10000)
        {
            if (_notifyIcon.Visible && _notifyIcon.Icon != null)
            {
                _notifyIcon.BalloonTipTitle = title;
                _notifyIcon.BalloonTipText = body;

                _notifyIcon.ShowBalloonTip(timeout);
            }
        }

        public static void ShowNotifyBaloon(string title, string body, ToolTipIcon icon, int timeout = 10000)
        {
            if (_notifyIcon.Visible && _notifyIcon.Icon != null)
            {
                _notifyIcon.BalloonTipTitle = title;
                _notifyIcon.BalloonTipText = body;
                _notifyIcon.BalloonTipIcon = icon;

                _notifyIcon.ShowBalloonTip(timeout);
            }
        }

        #endregion public static void ShowNotifyBaloon()
    }
}
