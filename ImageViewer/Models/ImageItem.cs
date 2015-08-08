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
