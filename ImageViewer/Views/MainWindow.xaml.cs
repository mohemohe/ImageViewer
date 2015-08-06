using System;
using System.Windows;
using System.Windows.Controls;
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
        public MainWindow()
        {
            InitializeComponent();

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

            Loaded += (sender, args) =>
            {
                VM.View = this;
            };
        }

        //HACK: 最高にやばい
        public dynamic VM
        {
            get { return DataContext; }
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as Image;
            if (image.Source != null)
            {
                var source = PresentationSource.FromVisual(this);
                var ct = source.CompositionTarget;
                var dpi = ct.TransformToDevice.Transform(new Point(1, 1)).X;
                VM.ImageRenderWidth = Convert.ToInt32(image.DesiredSize.Width * dpi);
                VM.ImageRenderHeight = Convert.ToInt32(image.DesiredSize.Height * dpi);
            }
        }

        private void MoveLeft(object sender, RoutedEventArgs e)
        {
            var template = TabControl.Template;
            var sv = (ScrollViewer)template.FindName("ScrollableTab", TabControl);
            sv.LineLeft();
        }

        private void MoveRight(object sender, RoutedEventArgs e)
        {
            var template = TabControl.Template;
            var sv = (ScrollViewer)template.FindName("ScrollableTab", TabControl);
            sv.LineRight();
        }
    }
}