using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageViewer.Views
{
    /*
     * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    ///     MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string imageUri)
        {
            InitializeComponent();

            VM.ImageUri = imageUri;

            StateChanged += (sender, args) =>
            {
                if (WindowState == WindowState.Maximized)
                {
                    // なんでずれるのかわからない
                    WindowRoot.Margin = new Thickness(SystemParameters.ResizeFrameVerticalBorderWidth*2);
                }
                else
                {
                    WindowRoot.Margin = new Thickness(0);
                }
            };

            // この処理は本来バインディングしてVM側に書くべきだけどDPIの算出が面倒くさくなるのでここに
            Image.SizeChanged += (sender, args) =>
            {
                if (Image.Source != null && Image.Source.Metadata != null)
                {
                    if (VM.FallbackImage == null)
                    {
                        var image = Image.Source.Clone() as BitmapSource;
                        if (image != null)
                        {
                            var stride = image.PixelWidth*((image.Format.BitsPerPixel + 7)/8);
                            var pixels = new byte[image.PixelHeight*stride];
                            image.CopyPixels(pixels, stride, 0);

                            try
                            {
                                VM.FallbackImage = BitmapSource.Create(
                                    image.PixelWidth,
                                    image.PixelHeight,
                                    image.DpiX,
                                    image.DpiY,
                                    image.Format,
                                    null,
                                    pixels,
                                    stride);
                            }
                            catch
                            {
                                try
                                {
                                    VM.FallbackImage = BitmapSource.Create(
                                    image.PixelWidth,
                                    image.PixelHeight,
                                    image.DpiX,
                                    image.DpiY,
                                    image.Format,
                                    new BitmapPalette(image, Convert.ToInt32(Math.Pow(2, image.Format.BitsPerPixel))),
                                    pixels,
                                    stride);
                                }
                                catch { }
                            }
                            VM.FallbackImageUri = imageUri;
                        }
                    }
                    var source = PresentationSource.FromVisual(this);
                    var ct = source.CompositionTarget;
                    var dpi = ct.TransformToDevice.Transform(new Point(1, 1)).X;
                    VM.ImageRenderWidth = Convert.ToInt32(Image.DesiredSize.Width*dpi);
                    VM.ImageRenderHeight = Convert.ToInt32(Image.DesiredSize.Height*dpi);
                }
            };
        }

        // 最高にやばい
        private dynamic VM
        {
            get { return DataContext; }
        }
    }
}