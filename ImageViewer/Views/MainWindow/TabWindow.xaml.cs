using System;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImageViewer.Helpers;
using ImageViewer.Models;
using ImageViewer.ViewModels;
using Image = System.Windows.Controls.Image;
using POINT = System.Drawing.Point;

namespace ImageViewer.Views.MainWindow
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
    // ReSharper disable once RedundantExtendsListEntry
    public partial class TabWindow : WindowBase
    {
        private bool _isMove;
        private bool _isTranslate;

        private POINT _mousePosition;
        public double DPI = 1;

        public TabWindow()
        {
            InitializeComponent();

            StateChanged +=
                (sender, args) =>
                {
                    WindowRoot.Margin = WindowState == WindowState.Maximized
                        ? new Thickness(SystemParameters.ResizeFrameVerticalBorderWidth*2)
                        : new Thickness(0);
                };

            Loaded += (sender, args) =>
            {
                var fromVisual = PresentationSource.FromVisual(this);
                if (fromVisual != null && fromVisual.CompositionTarget != null)
                {
                    var presentationSource = PresentationSource.FromVisual(this);
                    if (presentationSource?.CompositionTarget != null)
                        DPI = presentationSource.CompositionTarget.TransformToDevice.Transform(new Point(1, 1)).X;
                }

                VM.View = this;
                Win32Helper.MainWindowAppendMenu(Win32Helper.MenuFlags.SEPARATOR, @"", null);
                Win32Helper.MainWindowAppendMenu(Win32Helper.MenuFlags.STRING, @"Force Full GC", () =>
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
                });
            };

            Closing += (sender, args) => { Config.WindowPosition = RestoreBounds; };
        }

        //HACK: 最高にやばい
        public MainWindowViewModel VM
        {
            get { return DataContext as MainWindowViewModel; }
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as Image;
            if (image?.Source != null)
            {
                var zoom = VM.DeferredImageItems[VM.SelectedIndex].Zoom;
                VM.ImageRenderWidth = Convert.ToInt32(image.DesiredSize.Width*DPI*zoom);
                VM.ImageRenderHeight = Convert.ToInt32(image.DesiredSize.Height*DPI*zoom);
            }
        }

        private void MoveLeft(object sender, RoutedEventArgs e)
        {
            var template = TabControl.Template;
            var sv = (ScrollViewer) template.FindName("ScrollableTab", TabControl);
            sv.LineLeft();
        }

        private void MoveRight(object sender, RoutedEventArgs e)
        {
            var template = TabControl.Template;
            var sv = (ScrollViewer) template.FindName("ScrollableTab", TabControl);
            sv.LineRight();
        }

        private void Zoom(object sender, MouseWheelEventArgs e)
        {
            var maxSize = int.MaxValue/2.0/Math.Sqrt(2);
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

            VM.DeferredImageItems[VM.SelectedIndex].Zoom += (Math.Sign(e.Delta)*
                                                             VM.DeferredImageItems[VM.SelectedIndex].Zoom*0.1)/DPI;
            Image_SizeChanged(((Image) ((Grid) sender).Children[0]), null);
        }

        private void StartTranslate(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Win32Helper.GetCursorPos(out _mousePosition);
                Win32Helper.ShowCursor(false);
                _isTranslate = true;
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {
                VM.DeferredImageItems[VM.SelectedIndex].Zoom = 1.0;
                VM.DeferredImageItems[VM.SelectedIndex].Translate.X = 0.5;
                VM.DeferredImageItems[VM.SelectedIndex].Translate.Y = 0.5;
                Image_SizeChanged(((Image) ((Grid) sender).Children[0]), null);
            }
        }

        private void StopTranslate(object sender, MouseButtonEventArgs e)
        {
            if (_isTranslate)
            {
                _isTranslate = false;
                Win32Helper.SetCursorPos(_mousePosition.X, _mousePosition.Y);
                Win32Helper.ShowCursor(true);
            }
        }

        private void Translate(object sender, MouseEventArgs e)
        {
            if (!_isMove)
            {
                _isMove = true;
                if (_isTranslate)
                {
                    POINT currentPosition;
                    Win32Helper.GetCursorPos(out currentPosition);
                    Win32Helper.SetCursorPos(_mousePosition.X, _mousePosition.Y);

                    var longerLength = VM.ImageRenderWidth > VM.ImageRenderHeight
                        ? VM.ImageRenderWidth
                        : VM.ImageRenderHeight;

                    VM.DeferredImageItems[VM.SelectedIndex].Translate.X +=
                        ((currentPosition.X - _mousePosition.X)*Config.MouseSensibility)/((double) longerLength/VM.Zoom)*
                        (DPI*(100.0/VM.Zoom));
                    VM.DeferredImageItems[VM.SelectedIndex].Translate.Y +=
                        ((currentPosition.Y - _mousePosition.Y)*Config.MouseSensibility)/((double) longerLength/VM.Zoom)*
                        (DPI*(100.0/VM.Zoom));
                }
                _isMove = false;
            }
        }

        private void CaptionBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 1)
                {
                    try
                    {
                        DragMove();
                    }
                        // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
                else
                {
                    WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                POINT currentPosition;
                Win32Helper.GetCursorPos(out currentPosition);
                Win32Helper.ShowContextMenu(currentPosition);
            }
        }

        private void Tab_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Focus();

            if (e.Delta > 0)
            {
                if (VM.SelectedIndex != 0)
                {
                    VM.SelectedIndex--;
                    MoveLeft(null, null);
                }
            }
            else
            {
                if (VM.SelectedIndex < VM.DeferredImageItems.Count)
                {
                    VM.SelectedIndex++;
                    MoveRight(null, null);
                }
            }
        }

        private void Tab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                var tab = sender as Border;
                var item = (ImageItem) tab?.DataContext;
                if (tab != null)
                {
                    var index = TabControl.Items.IndexOf(item);
                    VM.TabClose(index);
                }
                e.Handled = true;
            }
        }
    }
}