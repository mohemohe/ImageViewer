using ImageViewer.Helpers;
using ImageViewer.Models;
using ImageViewer.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POINT = System.Drawing.Point;
using Image = System.Windows.Controls.Image;

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
        private bool _isTranslate = false;

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
                Win32Helper.MainWindowAppendMenu(Win32Helper.MenuFlags.SEPARATOR, @"", null);
                Win32Helper.MainWindowAppendMenu(Win32Helper.MenuFlags.STRING, @"Force Full GC", new Action(() =>
                {
                    var selectedIndex = VM.SelectedIndex;
                    //HACK: 2ループ（4回）GCを呼び出すと回収された　何故？
                    //NOTE: GCLargeObjectHeapCompactionMode.CompactOnce か GC.WaitForPendingFinalizers() が2回必要？
                    for (var i = 0; i < 2; i++)
                    {
                        var tmpImageItems = new ObservableCollection<ImageItem>(VM.DeferredImageItems);
                        VM.DeferredImageItems = null;
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        VM.DeferredImageItems = tmpImageItems;
                    }
                    VM.SelectedIndex = selectedIndex;
                }));
            };
        }

        //HACK: 最高にやばい
        public MainWindowViewModel VM
        {
            get { return DataContext as MainWindowViewModel; }
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as Image;
            if (image.Source != null)
            {
                var source = PresentationSource.FromVisual(this);
                var ct = source.CompositionTarget;
                var dpi = ct.TransformToDevice.Transform(new Point(1, 1)).X;
                var zoom = VM.DeferredImageItems[VM.SelectedIndex].Zoom;
                VM.ImageRenderWidth = Convert.ToInt32(image.DesiredSize.Width * dpi * zoom);
                VM.ImageRenderHeight = Convert.ToInt32(image.DesiredSize.Height * dpi * zoom);
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

        private void Zoom(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var maxSize = int.MaxValue / 2.0 / Math.Sqrt(2);
            var minSize = 0.01;
            if (Math.Sign(e.Delta) > 0)
            {
                if (VM.ImageRenderWidth > maxSize || VM.ImageRenderHeight > maxSize || VM.Zoom > maxSize)
                {
                    return;
                }
            }
            else
            {
                if (VM.Zoom < minSize)
                {
                    return;
                }
            }

            VM.DeferredImageItems[VM.SelectedIndex].Zoom += Math.Sign(e.Delta) * VM.DeferredImageItems[VM.SelectedIndex].Zoom * 0.1;
            Image_SizeChanged(((Image)((Grid)sender).Children[0]), null);
        }

        POINT _mousePosition;
        bool _isMove;

        private void StartTranslate(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Win32Helper.GetCursorPos(out _mousePosition);
                Win32Helper.ShowCursor(false);
                _isTranslate = true;
            }
            else if(e.ChangedButton == MouseButton.Middle)
            {
                VM.DeferredImageItems[VM.SelectedIndex].Zoom = 1.0;
                VM.DeferredImageItems[VM.SelectedIndex].Translate.X = 0.5;
                VM.DeferredImageItems[VM.SelectedIndex].Translate.Y = 0.5;
                Image_SizeChanged(((Image)((Grid)sender).Children[0]), null);
            }
        }

        private void StopTranslate(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isTranslate)
            {
                _isTranslate = false;
                Win32Helper.SetCursorPos(_mousePosition.X, _mousePosition.Y);
                Win32Helper.ShowCursor(true);
            }
        }

        private void Translate(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isMove)
            {
                _isMove = true;
                if (_isTranslate)
                {
                    POINT currentPosition;
                    Win32Helper.GetCursorPos(out currentPosition);
                    Win32Helper.SetCursorPos(_mousePosition.X, _mousePosition.Y);
                    VM.DeferredImageItems[VM.SelectedIndex].Translate.X += currentPosition.X - _mousePosition.X;
                    VM.DeferredImageItems[VM.SelectedIndex].Translate.Y += currentPosition.Y - _mousePosition.Y;
                }
                _isMove = false;
            }
        }

        private void CaptionBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 1)
                {
                    try
                    {
                        this.DragMove();
                    }
                    catch { }
                }
                else
                {
                    if(this.WindowState == WindowState.Maximized)
                    {
                        this.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;
                    }
                }
            }
            else if(e.ChangedButton == MouseButton.Right)
            {
                POINT currentPosition;
                Win32Helper.GetCursorPos(out currentPosition);
                Win32Helper.ShowContextMenu(currentPosition);
            }
        }
    }
}