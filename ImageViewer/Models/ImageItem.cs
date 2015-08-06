using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public int Width { get { return (int)base.Bitmap.Width; } }
        public int Height { get { return (int)base.Bitmap.Height; } }

        public ImageItem(string imageUri, string originalUri = null)
        {
            ImageUri = imageUri;
            OriginalUri = originalUri;
            Name = Path.GetFileName(ImageUri.Replace(@":orig", @""));
        }

        public async new Task<BitmapImage> DownloadDataAsync(string imageUri = null, string originalUri = null)
        {
            return (imageUri != null ? 
                await base.DownloadDataAsync(imageUri, originalUri) :
                await base.DownloadDataAsync(ImageUri, OriginalUri));
        }
    }
}
