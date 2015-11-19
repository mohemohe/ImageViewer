using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HUSauth.Helpers;
using ImageViewer.Helpers;
using ImageViewer.Models;
using ImageViewer.ViewModels;
using Image = System.Windows.Controls.Image;

namespace ImageViewer.Views.ViewWindow
{
    public class WindowBase : Window
    {
        private bool _isMove;
        private bool _isTranslate;

        private System.Drawing.Point _mousePosition;
        public double DPI = 1;

        protected void Initialize(dynamic window, bool isCreateNotifyIcon = false)
        {
            StateChanged +=
                (sender, args) =>
                {
                    window.WindowRoot.Margin = WindowState == WindowState.Maximized
                        ? new Thickness(SystemParameters.ResizeFrameVerticalBorderWidth * 2)
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

                window.VM.View = window;
                Win32Helper.MainWindowAppendMenu(Win32Helper.MenuFlags.SEPARATOR, @"", null);
                Win32Helper.MainWindowAppendMenu(Win32Helper.MenuFlags.STRING, @"Force Full GC", () =>
                {
                    var selectedIndex = VM.SelectedIndex;
                    ForceFullGC();
                    VM.SelectedIndex = selectedIndex;
                });

                if (isCreateNotifyIcon)
                {
                    NotifyIconHelper.Initialize(window);
                }
            };

            Closing += (sender, args) =>
            {
                Config.WindowPosition = RestoreBounds;
                if (NotifyIconHelper.Window != null)
                {
                    args.Cancel = !NotifyIconHelper.IsClosable;
                    NotifyIconHelper.Close();
                }
            };
        }

        //HACK: 最高にやばい
        public MainWindowViewModel VM
        {
            get { return DataContext as MainWindowViewModel; }
        }

        protected void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as System.Windows.Controls.Image;
            if (image?.Source != null)
            {
                var zoom = VM.DeferredImageItems[VM.SelectedIndex].Zoom;
                VM.ImageRenderWidth = Convert.ToInt32(image.DesiredSize.Width * DPI * zoom);
                VM.ImageRenderHeight = Convert.ToInt32(image.DesiredSize.Height * DPI * zoom);
            }
        }

        protected void Zoom(object sender, MouseWheelEventArgs e)
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

            VM.DeferredImageItems[VM.SelectedIndex].Zoom += (Math.Sign(e.Delta) *
                                                             VM.DeferredImageItems[VM.SelectedIndex].Zoom * 0.1) / DPI;
            Image_SizeChanged(((System.Windows.Controls.Image)((Grid)sender).Children[0]), null);
        }

        protected void StartTranslate(object sender, MouseButtonEventArgs e)
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
                Image_SizeChanged(((Image)((Grid)sender).Children[0]), null);
            }
        }

        protected void StopTranslate(object sender, MouseButtonEventArgs e)
        {
            if (_isTranslate)
            {
                _isTranslate = false;
                Win32Helper.SetCursorPos(_mousePosition.X, _mousePosition.Y);
                Win32Helper.ShowCursor(true);
            }
        }

        protected void Translate(object sender, MouseEventArgs e)
        {
            if (!_isMove)
            {
                _isMove = true;
                if (_isTranslate)
                {
                    System.Drawing.Point currentPosition;
                    Win32Helper.GetCursorPos(out currentPosition);
                    Win32Helper.SetCursorPos(_mousePosition.X, _mousePosition.Y);

                    var longerLength = VM.ImageRenderWidth > VM.ImageRenderHeight
                        ? VM.ImageRenderWidth
                        : VM.ImageRenderHeight;

                    VM.DeferredImageItems[VM.SelectedIndex].Translate.X +=
                        ((currentPosition.X - _mousePosition.X) * Config.MouseSensibility) / ((double)longerLength / VM.Zoom) *
                        (DPI * (100.0 / VM.Zoom));
                    VM.DeferredImageItems[VM.SelectedIndex].Translate.Y +=
                        ((currentPosition.Y - _mousePosition.Y) * Config.MouseSensibility) / ((double)longerLength / VM.Zoom) *
                        (DPI * (100.0 / VM.Zoom));
                }
                _isMove = false;
            }
        }

        protected void CaptionBar_MouseDown(object sender, MouseButtonEventArgs e)
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
                System.Drawing.Point currentPosition;
                Win32Helper.GetCursorPos(out currentPosition);
                Win32Helper.ShowContextMenu(currentPosition);
            }
        }


        protected void Tab_PreviewMouseDown(dynamic window, object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                var tab = sender as Border;
                var item = (ImageItem)tab?.DataContext;
                if (tab != null)
                {
                    var index = window.TabControl.Items.IndexOf(item);
                    VM.TabClose(index);
                }
                e.Handled = true;
            }
        }

        internal void ForceFullGC()
        {
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
        }
    }
}
