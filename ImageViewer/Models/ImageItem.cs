using ImageViewer.Helpers;
using Livet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageViewer.Models
{
    public class ImageItem : Image
    {
        public string Name { get; set; }
        public string OriginalUri { get; private set; }
        public string ImageUri { get; private set; }

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

        public int Width { get { return (base.Bitmap != null) ? (int)base.Bitmap.Width : 0 ; } }
        public int Height { get { return (base.Bitmap != null) ? (int)base.Bitmap?.Height : 0 ; } }

        public ImageItem(string imageUri, string originalUri = null)
        {
            ImageUri = imageUri;
            OriginalUri = originalUri;
            Name = Path.GetFileName(ImageUri.Replace(@":orig", @""));
        }

        public async new Task<BitmapImage> DownloadDataAsync(string imageUri = null, string originalUri = null)
        {
            BitmapImage bi;
            try {
                if (imageUri != null)
                {
                    bi = await base.DownloadDataAsync(imageUri, originalUri);
                }
                else
                {
                    bi = await base.DownloadDataAsync(ImageUri, OriginalUri);
                }
            }
            catch (WebException)
            {
                bi = null;
            }

            return bi;
        }
    }
}
