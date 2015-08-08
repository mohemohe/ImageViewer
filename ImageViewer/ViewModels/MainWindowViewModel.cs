using Livet;
using Livet.Commands;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Livet.Converters;
using ImageViewer.Models;
using System.Collections.ObjectModel;
using Livet.Messaging.Windows;
using ImageViewer.Views;
using System.Windows.Controls;

namespace ImageViewer.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ
         *
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *
         * を使用してください。
         *
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         *
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         *
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         *
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         *
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         *
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        public void Initialize()
        {

        }

        public async void AddTab(string imageUri, string originalUri = null)
        {
            ImageItems.Add(new ImageItem(imageUri, originalUri));
            await ImageItems[ImageItems.Count - 1].DownloadDataAsync();
            DeferredImageItems = new ObservableCollection<ImageItem>(ImageItems);

            SelectedIndex = DeferredImageItems.Count - 1;

            if (View.WindowState == WindowState.Minimized)
            {
                View.WindowState = WindowState.Normal;
            }
            for(var i = 0; i < 50; i++)
            {
                if (Application.Current.MainWindow.Activate())
                {
                    break;
                }
            }
            
            var template = View.TabControl.Template;
            var sv = (ScrollViewer)template.FindName("ScrollableTab", View.TabControl);
            sv.ScrollToRightEnd();
        }

        private void CalcZoom()
        {
            if (_SelectedIndex != -1 && DeferredImageItems[_SelectedIndex].Bitmap != null)
            {
                var imageSize = DeferredImageItems[_SelectedIndex].Bitmap.Width;
                var renderSize = ImageRenderWidth;
                Zoom = Convert.ToInt32((renderSize/imageSize)*100);
            }
        }

        #region View変更通知プロパティ
        private MainWindow _View;

        public MainWindow View
        {
            get
            { return _View; }
            set
            { 
                if (_View == value)
                    return;
                _View = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ステータスバーにバインディング

        #region ImageRenderWidth変更通知プロパティ

        private int _ImageRenderWidth;

        public int ImageRenderWidth
        {
            get { return _ImageRenderWidth; }
            set
            {
                if (_ImageRenderWidth == value)
                    return;
                _ImageRenderWidth = value;
                RaisePropertyChanged();
                CalcZoom();
            }
        }

        #endregion ImageRenderWidth変更通知プロパティ

        #region ImageRenderHeight変更通知プロパティ
        private int _ImageRenderHeight;

        public int ImageRenderHeight
        {
            get
            { return _ImageRenderHeight; }
            set
            { 
                if (_ImageRenderHeight == value)
                    return;
                _ImageRenderHeight = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region SelectedImageWidth変更通知プロパティ

        private int _SelectedImageWidth;

        public int SelectedImageWidth
        {
            get { return _SelectedImageWidth; }
            set
            {
                if (_SelectedImageWidth == value)
                    return;
                _SelectedImageWidth = value;
                RaisePropertyChanged();
            }
        }

        #endregion ImageRenderWidth変更通知プロパティ

        #region SelectedImageHeight変更通知プロパティ

        private int _SelectedImageHeight;

        public int SelectedImageHeight
        {
            get { return _SelectedImageHeight; }
            set
            {
                if (_SelectedImageHeight == value)
                    return;
                _SelectedImageHeight = value;
                RaisePropertyChanged();
                CalcZoom();
            }
        }

        #endregion ImageRenderHeight変更通知プロパティ

        #region Zoom変更通知プロパティ

        private int _Zoom;

        public int Zoom
        {
            get { return _Zoom; }
            set
            {
                if (_Zoom == value)
                    return;
                _Zoom = value;
                RaisePropertyChanged();
            }
        }

        #endregion Zoom変更通知プロパティ

        #endregion ステータスバーにバインディング

        #region SelectedIndex変更通知プロパティ

        private int _SelectedIndex;

        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                _SelectedIndex = value;
                if (value != -1)
                {
                    SelectedImageWidth = DeferredImageItems[value].Width;
                    SelectedImageHeight = DeferredImageItems[value].Height;
                    CalcZoom();
                }
                RaisePropertyChanged();
            }
        }

        #endregion SelectedIndex変更通知プロパティ

        #region ImageItems変更通知プロパティ

        private List<ImageItem> _ImageItems = new List<ImageItem>();

        public List<ImageItem> ImageItems
        {
            get { return _ImageItems; }
            set
            {
                if (_ImageItems == value)
                    return;
                _ImageItems = value;
                RaisePropertyChanged();
            }
        }

        #endregion ImageItems変更通知プロパティ

        #region DeferredImageItems変更通知プロパティ

        private ObservableCollection<ImageItem> _DeferredImageItems = new ObservableCollection<ImageItem>();

        public ObservableCollection<ImageItem> DeferredImageItems
        {
            get { return _DeferredImageItems; }
            set
            {
                if (_DeferredImageItems == value)
                    return;
                _DeferredImageItems = value;
                RaisePropertyChanged();
            }
        }

        #endregion DeferredImageItems変更通知プロパティ

        #region SaveImageCommand

        private ViewModelCommand _SaveImageCommand;

        public ViewModelCommand SaveImageCommand
        {
            get
            {
                if (_SaveImageCommand == null)
                {
                    _SaveImageCommand = new ViewModelCommand(SaveImage);
                }
                return _SaveImageCommand;
            }
        }

        public void SaveImage()
        {
            var fileName = Path.GetFileNameWithoutExtension(DeferredImageItems[SelectedIndex].Name);
            var ext = Path.GetExtension(DeferredImageItems[SelectedIndex].Name);

            var tmpList = new List<string>
            {
                "JPEG(*.jpg;*.jpeg)|*.jpg;*.jpeg|",
                "Bitmap(*.bmp)|*.bmp|",
                "PNG(*.png)|*.png|",
                "GIF(*.gif)|*.gif|"
            };

            var filterList = new List<string>();
            filterList.AddRange(tmpList.Where(x => x.Contains(ext)));

            var filter = "";
            filterList.ForEach(x => filter += x);
            filter = filter.Remove(filter.Length - 1, 1);
            filter += "|All Files(*.*)|*.*";

            var message = new SavingFileSelectionMessage("Save")
            {
                AddExtension = true,
                FileName = fileName + ext,
                Filter = filter,
            };
            Messenger.Raise(message);
            if (message.Response == null)
            {
                return;
            }

            DeferredImageItems[SelectedIndex].Save(message.Response[0]);
        }
        #endregion SaveImageCommand

        #region CopyToClipboardCommand
        private ViewModelCommand _CopyToClipboardCommand;

        public ViewModelCommand CopyToClipboardCommand
        {
            get
            {
                if (_CopyToClipboardCommand == null)
                {
                    _CopyToClipboardCommand = new ViewModelCommand(CopyToClipboard);
                }
                return _CopyToClipboardCommand;
            }
        }

        public void CopyToClipboard()
        {
            Clipboard.SetImage(DeferredImageItems[SelectedIndex].Bitmap);
        }
        #endregion CopyToClipboardCommand

        #region OpenInBrowserCommand
        private ViewModelCommand _OpenInBrowserCommand;

        public ViewModelCommand OpenInBrowserCommand
        {
            get
            {
                if (_OpenInBrowserCommand == null)
                {
                    _OpenInBrowserCommand = new ViewModelCommand(OpenInBrowser);
                }
                return _OpenInBrowserCommand;
            }
        }

        public void OpenInBrowser()
        {
            Process.Start(DeferredImageItems[SelectedIndex].ImageUri);
        }
        #endregion OpenInBrowserCommand

        #region SearchByGoogleCommand
        private ViewModelCommand _SearchByGoogleCommand;

        public ViewModelCommand SearchByGoogleCommand
        {
            get
            {
                if (_SearchByGoogleCommand == null)
                {
                    _SearchByGoogleCommand = new ViewModelCommand(SearchByGoogle);
                }
                return _SearchByGoogleCommand;
            }
        }

        public void SearchByGoogle()
        {
            Process.Start(@"https://www.google.com/searchbyimage?image_url=" + DeferredImageItems[SelectedIndex].ImageUri);
        }
        #endregion

        #region TabCloseCommand
        private ListenerCommand<int> _TabCloseCommand;

        public ListenerCommand<int> TabCloseCommand
        {
            get
            {
                if (_TabCloseCommand == null)
                {
                    _TabCloseCommand = new ListenerCommand<int>(TabClose);
                }
                return _TabCloseCommand;
            }
        }

        public void TabClose(int parameter)
        {
            if (DeferredImageItems.Count == 1)
            {
                Messenger.Raise(new WindowActionMessage(WindowAction.Close, "WindowMessage"));
                return;
            }

            var currentIndex = SelectedIndex;
            ImageItems.RemoveAt(parameter);
            DeferredImageItems = new ObservableCollection<ImageItem>(ImageItems);
            SelectedIndex = currentIndex < DeferredImageItems.Count ? currentIndex : DeferredImageItems.Count - 1;
        }
        #endregion

    }
}