﻿using ImageViewer.Models;
using ImageViewer.Views;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HUSauth.Helpers;
using ImageViewer.Views.ViewWindow;

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

        public bool Initialized { get; set; }

        public void Initialize()
        {
            Initialized = true;
        }

        public async void AddTab(string imageUri, string originalUri = null)
        {
            NotifyIconHelper.TryShow();
            // HACK: タブを切り替えたあとにSelectedIndexを変更してもアクティブなタブが変更されない
            View.Focus();

            DeferredImageItems.Add(new ImageItem(imageUri, originalUri));
            SelectedIndex = DeferredImageItems.Count - 1;

            if (View.WindowState == WindowState.Minimized)
            {
                View.WindowState = WindowState.Normal;
            }
            for (var i = 0; i < 50; i++)
            {
                if (Application.Current.MainWindow.Activate())
                {
                    break;
                }
            }

            if (View.GetType() == typeof (TabWindow))
            {
                var template = View.TabControl.Template;
                var sv = (ScrollViewer) template.FindName("ScrollableTab", View.TabControl);
                sv.ScrollToRightEnd();
            }
            else
            {
                SelectedIndex = 0;
            }

            await DeferredImageItems[DeferredImageItems.Count - 1].DownloadDataAsync(
                DeferredImageItems[DeferredImageItems.Count - 1].ImageUri,
                DeferredImageItems[DeferredImageItems.Count - 1].OriginalUri);
            SelectedImageWidth = DeferredImageItems[SelectedIndex].Width;
            SelectedImageHeight = DeferredImageItems[SelectedIndex].Height;
            DeferredImageItems[DeferredImageItems.Count - 1].Zoom = 1.0;
        }

        private void CalcZoom()
        {
            if (_SelectedIndex != -1 && DeferredImageItems[_SelectedIndex].Bitmap != null)
            {
                var imageSize = DeferredImageItems[_SelectedIndex].Width;
                var renderSize = ImageRenderWidth;
                if (imageSize == 0)
                {
                    return;
                }
                var zoomBase = (renderSize/(double) imageSize);
                Zoom = Convert.ToInt32(zoomBase*100);
            }
        }

        private void CalcActualZoom()
        {
            if (_SelectedIndex != -1 && DeferredImageItems[_SelectedIndex].Bitmap != null)
            {
                var imageSize = DeferredImageItems[_SelectedIndex].Width;
                var renderSize = ImageRenderWidth;
                if (imageSize == 0)
                {
                    return;
                }
                var zoomBase = (renderSize/(double) imageSize);
                DeferredImageItems[_SelectedIndex].ActualZoom = zoomBase;
            }
        }

        #region View変更通知プロパティ

        private dynamic _View;

        public dynamic View
        {
            get { return _View; }
            set
            {
                if (_View == value)
                    return;
                _View = value;
                RaisePropertyChanged();
            }
        }

        #endregion View変更通知プロパティ

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
                CalcActualZoom();
            }
        }

        #endregion ImageRenderWidth変更通知プロパティ

        #region ImageRenderHeight変更通知プロパティ

        private int _ImageRenderHeight;

        public int ImageRenderHeight
        {
            get { return _ImageRenderHeight; }
            set
            {
                if (_ImageRenderHeight == value)
                    return;
                _ImageRenderHeight = value;
                RaisePropertyChanged();
            }
        }

        #endregion ImageRenderHeight変更通知プロパティ

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

        #endregion SelectedImageWidth変更通知プロパティ

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

        #endregion SelectedImageHeight変更通知プロパティ

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
                if (DeferredImageItems != null && value < DeferredImageItems.Count)
                {
                    _SelectedIndex = value;

                    if (value != -1)
                    {
                        SelectedItemName = DeferredImageItems[value].Name;
                        SelectedImageWidth = DeferredImageItems[value].Width;
                        SelectedImageHeight = DeferredImageItems[value].Height;
                        CalcZoom();
                        
                    }
                }
                RaisePropertyChanged();
            }
        }

        #endregion SelectedIndex変更通知プロパティ

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

        #region SelectedItemName変更通知プロパティ
        private string _SelectedItemName = string.Empty;

        public string SelectedItemName
        {
            get
            { return _SelectedItemName; }
            set
            { 
                if (DeferredImageItems == null || DeferredImageItems.Count == 0 || SelectedIndex == -1)
                {
                    return;
                }
                _SelectedItemName = @" - " + value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region MaximizeZoomCommand

        private ViewModelCommand _MaximizeZoomCommand;

        public ViewModelCommand MaximizeZoomCommand
        {
            get
            {
                if (_MaximizeZoomCommand == null)
                {
                    _MaximizeZoomCommand = new ViewModelCommand(MaximizeZoom);
                }
                return _MaximizeZoomCommand;
            }
        }

        public void MaximizeZoom()
        {
            DeferredImageItems[SelectedIndex].Zoom = 1.0;
            CalcZoom();
        }

        #endregion MaximizeZoomCommand

        #region ResetZoomCommand

        private ViewModelCommand _ResetZoomCommand;

        public ViewModelCommand ResetZoomCommand
        {
            get
            {
                if (_ResetZoomCommand == null)
                {
                    _ResetZoomCommand = new ViewModelCommand(ResetZoom);
                }
                return _ResetZoomCommand;
            }
        }

        public void ResetZoom()
        {
            DeferredImageItems[SelectedIndex].Zoom /= DeferredImageItems[SelectedIndex].ActualZoom;
            Zoom = 100;
            ImageRenderWidth = DeferredImageItems[SelectedIndex].Width;
            ImageRenderHeight = DeferredImageItems[SelectedIndex].Height;
        }

        #endregion ResetZoomCommand

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
            var imageItem = DeferredImageItems[SelectedIndex];
            if (imageItem.Bitmap == null)
            {
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(imageItem.Name);
            var ext = Path.GetExtension(imageItem.FileExtension);

            var tmpList = new List<string>
            {
                "JPEG(*.jpg;*.jpeg;*.jfif)|*.jpg;*.jpeg;*.jfif|",
                "Bitmap(*.bmp)|*.bmp|",
                "PNG(*.png)|*.png|",
                "GIF(*.gif)|*.gif|"
            };

            var filterList = new List<string>();
            filterList.AddRange(tmpList.Where(x => ext != null && x.Contains(ext)));

            var filter = "";
            filterList.ForEach(x => filter += x);
            filter += "All Files(*.*)|*.*";

            var message = new SavingFileSelectionMessage("Save")
            {
                AddExtension = true,
                FileName = fileName + ext,
                Filter = filter
            };
            Messenger.Raise(message);
            if (message.Response == null)
            {
                return;
            }

            imageItem.Save(message.Response[0]);
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
            if (DeferredImageItems[SelectedIndex].Bitmap != null)
            {
                Clipboard.SetImage(DeferredImageItems[SelectedIndex].Bitmap);
            }
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
            var uri = (DeferredImageItems[SelectedIndex].IsError)
                ? DeferredImageItems[SelectedIndex].OriginalUri
                : DeferredImageItems[SelectedIndex].ImageUri;

            if (Config.DefaultBrowserPath == null)
            {
                Process.Start(uri);
            }
            else
            {
                var psi = new ProcessStartInfo {Arguments = uri, FileName = Config.DefaultBrowserPath};
                Process.Start(psi);
            }
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
            if (DeferredImageItems[SelectedIndex].Bitmap != null)
            {
                var uri = @"https://www.google.com/searchbyimage?image_url=" +
                          DeferredImageItems[SelectedIndex].ImageUri;

                if (Config.DefaultBrowserPath == null)
                {
                    Process.Start(uri);
                }
                else
                {
                    var psi = new ProcessStartInfo {Arguments = uri, FileName = Config.DefaultBrowserPath};
                    Process.Start(psi);
                }
            }
        }

        #endregion SearchByGoogleCommand

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
            SelectedIndex = -1;

            DeferredImageItems.RemoveAt(parameter);

            SelectedIndex = currentIndex < DeferredImageItems.Count
                ? currentIndex
                : DeferredImageItems.Count - 1;
        }

        #endregion TabCloseCommand

        #region OpenSettingsWindowCommand

        private ViewModelCommand _OpenSettingsWindowCommand;

        public ViewModelCommand OpenSettingsWindowCommand
        {
            get
            {
                if (_OpenSettingsWindowCommand == null)
                {
                    _OpenSettingsWindowCommand = new ViewModelCommand(OpenSettingsWindow);
                }
                return _OpenSettingsWindowCommand;
            }
        }

        public void OpenSettingsWindow()
        {
            Messenger.Raise(new TransitionMessage(new SettingsWindowViewModel(), "OpenMessage"));
        }

        #endregion OpenSettingsWindowCommand
    }
}