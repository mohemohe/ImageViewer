using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageViewer
{
    /// <summary>
    ///     MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string imageUri)
        {
            InitializeComponent();
            var image = new BitmapImage(new Uri(imageUri));
            image.DownloadCompleted += (sender, args) =>
            {
                Image.Source = image;
                Image.Width = image.PixelWidth;
                Image.Height = image.PixelHeight;
            };
        }
    }
}