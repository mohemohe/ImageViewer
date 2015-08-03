using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;

namespace ImageViewer.Models
{
    public class Image
    {
        public byte[] OriginalData { get; private set; }
        public Bitmap OriginalBitmap { get; private set; }

        public async Task<BitmapImage> DownloadDataAsync(string uri, string referer = null)
        {
            var wc = new WebClient();
            wc.Referer = referer?.ToString();
            OriginalData = await wc.DownloadDataTaskAsync(uri);

            var bi = new BitmapImage();
            using (var ms = new MemoryStream(OriginalData))
            {
                OriginalBitmap = new Bitmap(ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);

                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
            }

            return bi;
        }
    }
}
