using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using ImageViewer.Models;
using System.Diagnostics;
using System.Reflection;

namespace ImageViewer.ViewModels
{
    public class SettingsWindowViewModel : ViewModel
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

        #region 保持

        #region 全般

        #region IsEnablePseudoSingleInstance変更通知プロパティ
        private bool _IsEnablePseudoSingleInstance;

        public bool IsEnablePseudoSingleInstance
        {
            get
            { return _IsEnablePseudoSingleInstance; }
            set
            {
                if (_IsEnablePseudoSingleInstance == value)
                    return;
                _IsEnablePseudoSingleInstance = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsChildWindow変更通知プロパティ
        private bool _IsChildWindow;

        public bool IsChildWindow
        {
            get
            { return _IsChildWindow; }
            set
            {
                if (_IsChildWindow == value)
                    return;
                _IsChildWindow = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #endregion 全般

        #region ビューアー

        #region MouseSensibility変更通知プロパティ
        private double _MouseSensibility;

        public double MouseSensibility
        {
            get
            { return _MouseSensibility; }
            set
            { 
                if (_MouseSensibility == value)
                    return;
                _MouseSensibility = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #endregion

        #region サイト別設定

        #region IsFallbackTwitterGifMovie変更通知プロパティ
        private bool _IsFallbackTwitterGifMovie;

        public bool IsFallbackTwitterGifMovie
        {
            get
            { return _IsFallbackTwitterGifMovie; }
            set
            {
                if (_IsFallbackTwitterGifMovie == value)
                    return;
                _IsFallbackTwitterGifMovie = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsWarningTwitter30secMovie変更通知プロパティ
        private bool _IsWarningTwitter30secMovie;

        public bool IsWarningTwitter30secMovie
        {
            get
            { return _IsWarningTwitter30secMovie; }
            set
            {
                if (_IsWarningTwitter30secMovie == value)
                    return;
                _IsWarningTwitter30secMovie = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsUsePixivWebScraping変更通知プロパティ
        private bool _IsUsePixivWebScraping;

        public bool IsUsePixivWebScraping
        {
            get
            { return _IsUsePixivWebScraping; }
            set
            {
                if (_IsUsePixivWebScraping == value)
                    return;
                _IsUsePixivWebScraping = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PixivAccount変更通知プロパティ
        private PixivAccount _PixivAccount;

        public PixivAccount PixivAccount
        {
            get
            { return _PixivAccount; }
            set
            {
                if (_PixivAccount == value)
                    return;
                _PixivAccount = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #endregion サイト別設定

        #region 外部アプリケーション

        #region DefaultBrowserPath変更通知プロパティ
        private string _DefaultBrowserPath;

        public string DefaultBrowserPath
        {
            get
            { return _DefaultBrowserPath; }
            set
            { 
                if (_DefaultBrowserPath == value)
                    return;
                _DefaultBrowserPath = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #endregion 外部アプリケーション

        #endregion 保持

        public void Initialize()
        {
            GetVersion();
            LoadConfig();
        }

        private void GetVersion()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void LoadConfig()
        {
            DefaultBrowserPath = Config.DefaultBrowserPath ?? string.Empty;
            IsEnablePseudoSingleInstance = Config.IsEnablePseudoSingleInstance;
            IsChildWindow = Config.IsChildWindow;
            IsFallbackTwitterGifMovie = Config.IsFallbackTwitterGifMovie;
            IsWarningTwitter30secMovie = Config.IsWarningTwitter30secMovie;
            IsUsePixivWebScraping = Config.IsUsePixivWebScraping;
            PixivAccount = Config.PixivAccount;
            MouseSensibility = Config.MouseSensibility;
        }

        #region Version変更通知プロパティ
        private string _Version;

        public string Version
        {
            get
            { return _Version; }
            set
            { 
                if (_Version == value)
                    return;
                _Version = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region OKCommand
        private ViewModelCommand _OKCommand;

        public ViewModelCommand OKCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new ViewModelCommand(OK);
                }
                return _OKCommand;
            }
        }

        public void OK()
        {
            Apply();
            Cancel();
        }
        #endregion

        #region CancelCommand
        private ViewModelCommand _CancelCommand;

        public ViewModelCommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new ViewModelCommand(Cancel);
                }
                return _CancelCommand;
            }
        }

        public void Cancel()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "WindowMessage"));
        }
        #endregion

        #region ApplyCommand
        private ViewModelCommand _ApplyCommand;

        public ViewModelCommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                {
                    _ApplyCommand = new ViewModelCommand(Apply);
                }
                return _ApplyCommand;
            }
        }

        public void Apply()
        {
            Config.DefaultBrowserPath = File.Exists(DefaultBrowserPath) ? DefaultBrowserPath : null;
            Config.IsEnablePseudoSingleInstance = IsEnablePseudoSingleInstance;
            Config.IsChildWindow = IsChildWindow;
            Config.IsFallbackTwitterGifMovie = IsFallbackTwitterGifMovie;
            Config.IsWarningTwitter30secMovie = IsWarningTwitter30secMovie;
            Config.IsUsePixivWebScraping = IsUsePixivWebScraping;
            Config.PixivAccount = PixivAccount;
            Config.MouseSensibility = MouseSensibility;
        }
        #endregion

        #region SetDefaultBrowserCommand
        private ViewModelCommand _SetDefaultBrowserPathCommand;

        public ViewModelCommand SetDefaultBrowserPathCommand
        {
            get
            {
                if (_SetDefaultBrowserPathCommand == null)
                {
                    _SetDefaultBrowserPathCommand = new ViewModelCommand(SetDefaultBrowserPath);
                }
                return _SetDefaultBrowserPathCommand;
            }
        }

        public void SetDefaultBrowserPath()
        {
            var message = new OpeningFileSelectionMessage("Open") { Filter = "実行可能ファイル(*.exe)|*.exe" };
            Messenger.Raise(message);
            if (message.Response == null)
            {
                return;
            }

            DefaultBrowserPath = message.Response[0];
        }
        #endregion

    }
}
