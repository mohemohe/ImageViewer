using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageViewer.Models;

namespace ImageViewer.Views.ViewWindow
{
    /* 
	 * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// TabWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TabWindow : WindowBase
    {
        public TabWindow()
        {
            base.Initialize(this);
            InitializeComponent();
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
            base.Tab_PreviewMouseDown(this, sender, e);
        }
    }
}