using ImageViewer.Helpers;
using ImageViewer.Infrastructures;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageViewer.Models
{
    public class ImageItem : Image
    {
        public ImageItem(string imageUri, string originalUri = null)
        {
            ImageUri = imageUri;
            OriginalUri = originalUri;
            Name = Path.GetFileName(ImageUri.Replace(@":orig", @""));
            IsLoading = Visibility.Visible;
        }

        public string OriginalUri { get; private set; }
        public string ImageUri { get; private set; }
        public bool IsError { get; private set; }

        public int Width
        {
            get { return (!IsError && Bitmap != null) ? (int) Bitmap.Width : 0; }
        }

        public int Height
        {
            get { return (!IsError && Bitmap != null) ? (int) Bitmap?.Height : 0; }
        }

        public new async Task<BitmapImage> DownloadDataAsync(string imageUri, string originalUri)
        {
            var uri = imageUri;
            var bi = new BitmapImage();

            if (uri == @"{Pixiv}")
            {
                ImageUri = originalUri;
                OriginalUri = originalUri;

                var imageInfo = await PixivCrawler.GetImage(originalUri);
                if (imageInfo.ImageData != null)
                {
                    SetData(imageInfo.ImageData);
                    Name = Path.GetFileName(imageInfo.ImageUri);
                    IsLoading = Visibility.Hidden;
                    return (BitmapImage) Bitmap;
                }
                IsError = true;
            }

            // TODO: いくらなんでもここに書くのは汚いので後で移す
            IsError |= (Config.IsWarningTwitter30secMovie &&
                        (uri.StartsWith(@"https://pbs.twimg.com/ext_tw_video_thumb/") ||
                         uri.StartsWith(@"http://pbs.twimg.com/ext_tw_video_thumb/")));

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
                Bitmap = bi;
            }
            IsLoading = Visibility.Hidden;

            return bi;
        }

        #region Name変更通知プロパティ

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name == value)
                    return;
                _Name = value;
                RaisePropertyChanged();
            }
        }

        #endregion Name変更通知プロパティ

        #region IsLoading変更通知プロパティ

        private Visibility _IsLoading;

        public Visibility IsLoading
        {
            get { return _IsLoading; }
            set
            {
                if (_IsLoading == value)
                    return;
                _IsLoading = value;
                RaisePropertyChanged();
            }
        }

        #endregion IsLoading変更通知プロパティ

        #region Zoom変更通知プロパティ

        private double _Zoom = 1.0;

        public double Zoom
        {
            get { return _Zoom; }
            set
            {
                if (_Zoom == value)
                    return;
                _Zoom = value;
                RaisePropertyChanged();
            }
        }

        #endregion Zoom変更通知プロパティ

        #region Translate変更通知プロパティ

        private DimensionHelper.TwoDimension _Translate = new DimensionHelper.TwoDimension {X = 0.5, Y = 0.5};

        public DimensionHelper.TwoDimension Translate
        {
            get { return _Translate; }
            set
            {
                if (_Translate == value)
                    return;
                _Translate = value;
                RaisePropertyChanged();
            }
        }

        #endregion Translate変更通知プロパティ
    }
}