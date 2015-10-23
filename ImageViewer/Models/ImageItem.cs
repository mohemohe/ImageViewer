using ImageViewer.Helpers;
using Livet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageViewer.Models
{
    public class ImageItem : Image
    {
        public string Name { get; set; }
        public string OriginalUri { get; private set; }
        public string ImageUri { get; private set; }
        public bool IsError { get; private set; }

        #region IsLoading変更通知プロパティ
        private Visibility _IsLoading;

        public Visibility IsLoading
        {
            get
            { return _IsLoading; }
            set
            { 
                if (_IsLoading == value)
                    return;
                _IsLoading = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Zoom変更通知プロパティ
        private double _Zoom = 1.0;

        public double Zoom
        {
            get
            { return _Zoom; }
            set
            { 
                if (_Zoom == value)
                    return;
                _Zoom = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Translate変更通知プロパティ
        private DimensionHelper.TwoDimension _Translate = new DimensionHelper.TwoDimension { X = 0.5, Y = 0.5 };

        public DimensionHelper.TwoDimension Translate
        {
            get
            { return _Translate; }
            set
            { 
                if (_Translate == value)
                    return;
                _Translate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public int Width { get { return (!IsError && base.Bitmap != null) ? (int)base.Bitmap.Width : 0 ; } }
        public int Height { get { return (!IsError && base.Bitmap != null) ? (int)base.Bitmap?.Height : 0 ; } }

        public ImageItem(string imageUri, string originalUri = null)
        {
            ImageUri = imageUri;
            OriginalUri = originalUri;
            Name = Path.GetFileName(ImageUri.Replace(@":orig", @""));
            IsLoading = Visibility.Visible;
        }

        public async new Task<BitmapImage> DownloadDataAsync(string imageUri = null, string originalUri = null)
        {
            var uri = imageUri ?? ImageUri;
            BitmapImage bi = new BitmapImage();

            // TODO: いくらなんでもここに書くのは汚いので後で移す
            if(Config.IsWarningTwitter30secMovie && (uri.StartsWith(@"https://pbs.twimg.com/ext_tw_video_thumb/") || uri.StartsWith(@"http://pbs.twimg.com/ext_tw_video_thumb/")))
            {
                IsError = true;
            }

            try
            {
                if (!IsError)
                {
                    bi = await base.DownloadDataAsync(uri, originalUri);
                }
            }
            catch
            {
                IsError = true;
            }

            if (IsError)
            {
                bi = new BitmapImage(new Uri(@"pack://application:,,,/Resources/IcoMoon/warning.png", UriKind.Absolute));
                base.Bitmap = bi;
            }
            IsLoading = Visibility.Hidden;

            return bi;
        }


    }
}
