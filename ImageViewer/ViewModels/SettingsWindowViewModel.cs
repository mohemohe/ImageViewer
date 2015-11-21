using ImageViewer.Models;
using ImageViewer.Views;
using Livet;
using Livet.Commands;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using System.IO;
using System.Reflection;

namespace ImageViewer.ViewModels
{
    public class SettingsWindowViewModel : ViewModel
    {
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
            IsEnableAggressiveMode = Config.IsEnableAggressiveMode;
            IsDisableNotificationWhenAggressiveMode = Config.IsDisableNotificationWhenAggressiveMode;
            IsKeepingTabsWhenAggressiveMode = Config.IsKeepingTabsWhenAggressiveMode;
            IsChildWindow = Config.IsChildWindow;
            IsFallbackTwitterGifMovie = Config.IsFallbackTwitterGifMovie;
            IsWarningTwitter30secMovie = Config.IsWarningTwitter30secMovie;
            IsUsePixivWebScraping = Config.IsUsePixivWebScraping;
            PixivAccount = Config.PixivAccount;
            IsUseNicoSeigaWebScraping = Config.IsUseNicoSeigaWebScraping;
            SeigaAccount = Config.NicovideoAccount;
            MouseSensibility = Config.MouseSensibility;
        }

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

        #region View変更通知プロパティ

        private SettingsWindow _View;

        public SettingsWindow View
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

        #region 保持

        #region 全般

        #region IsEnablePseudoSingleInstance変更通知プロパティ

        private bool _IsEnablePseudoSingleInstance;

        public bool IsEnablePseudoSingleInstance
        {
            get { return _IsEnablePseudoSingleInstance; }
            set
            {
                if (_IsEnablePseudoSingleInstance == value)
                    return;
                _IsEnablePseudoSingleInstance = value;
                RaisePropertyChanged();
            }
        }

        #endregion IsEnablePseudoSingleInstance変更通知プロパティ

        #region IsEnableAggressiveMode変更通知プロパティ
        private bool _IsEnableAggressiveMode;

        public bool IsEnableAggressiveMode
        {
            get
            { return _IsEnableAggressiveMode; }
            set
            { 
                if (_IsEnableAggressiveMode == value)
                    return;
                _IsEnableAggressiveMode = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsDisableNotificationWhenAggressiveMode変更通知プロパティ
        private bool _IsDisableNotificationWhenAggressiveMode;

        public bool IsDisableNotificationWhenAggressiveMode
        {
            get
            { return _IsDisableNotificationWhenAggressiveMode; }
            set
            { 
                if (_IsDisableNotificationWhenAggressiveMode == value)
                    return;
                _IsDisableNotificationWhenAggressiveMode = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsKeepingTabsWhenAggressiveMode変更通知プロパティ
        private bool _IsKeepingTabsWhenAggressiveMode;

        public bool IsKeepingTabsWhenAggressiveMode
        {
            get
            { return _IsKeepingTabsWhenAggressiveMode; }
            set
            { 
                if (_IsKeepingTabsWhenAggressiveMode == value)
                    return;
                _IsKeepingTabsWhenAggressiveMode = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsChildWindow変更通知プロパティ

        private bool _IsChildWindow;

        public bool IsChildWindow
        {
            get { return _IsChildWindow; }
            set
            {
                if (_IsChildWindow == value)
                    return;
                _IsChildWindow = value;
                RaisePropertyChanged();
            }
        }

        #endregion IsChildWindow変更通知プロパティ

        #endregion 全般

        #region ビューアー

        #region MouseSensibility変更通知プロパティ

        private double _MouseSensibility;

        public double MouseSensibility
        {
            get { return _MouseSensibility; }
            set
            {
                if (_MouseSensibility == value)
                    return;
                _MouseSensibility = value;
                RaisePropertyChanged();
            }
        }

        #endregion MouseSensibility変更通知プロパティ

        #endregion ビューアー

        #region サイト別設定

        #region IsFallbackTwitterGifMovie変更通知プロパティ

        private bool _IsFallbackTwitterGifMovie;

        public bool IsFallbackTwitterGifMovie
        {
            get { return _IsFallbackTwitterGifMovie; }
            set
            {
                if (_IsFallbackTwitterGifMovie == value)
                    return;
                _IsFallbackTwitterGifMovie = value;
                RaisePropertyChanged();
            }
        }

        #endregion IsFallbackTwitterGifMovie変更通知プロパティ

        #region IsWarningTwitter30secMovie変更通知プロパティ

        private bool _IsWarningTwitter30secMovie;

        public bool IsWarningTwitter30secMovie
        {
            get { return _IsWarningTwitter30secMovie; }
            set
            {
                if (_IsWarningTwitter30secMovie == value)
                    return;
                _IsWarningTwitter30secMovie = value;
                RaisePropertyChanged();
            }
        }

        #endregion IsWarningTwitter30secMovie変更通知プロパティ

        #region IsUsePixivWebScraping変更通知プロパティ

        private bool _IsUsePixivWebScraping;

        public bool IsUsePixivWebScraping
        {
            get { return _IsUsePixivWebScraping; }
            set
            {
                if (_IsUsePixivWebScraping == value)
                    return;
                _IsUsePixivWebScraping = value;
                RaisePropertyChanged();
            }
        }

        #endregion IsUsePixivWebScraping変更通知プロパティ

        #region PixivAccount変更通知プロパティ

        private PixivAccount _PixivAccount;

        public PixivAccount PixivAccount
        {
            get { return _PixivAccount; }
            set
            {
                if (_PixivAccount == value)
                    return;
                _PixivAccount = value;
                RaisePropertyChanged();
            }
        }

        #endregion PixivAccount変更通知プロパティ

        #region IsUseNicoSeigaWebScraping変更通知プロパティ

        private bool _IsUseNicoSeigaWebScraping;

        public bool IsUseNicoSeigaWebScraping
        {
            get { return _IsUseNicoSeigaWebScraping; }
            set
            {
                if (_IsUseNicoSeigaWebScraping == value)
                    return;
                _IsUseNicoSeigaWebScraping = value;
                RaisePropertyChanged();
            }
        }

        #endregion IsUseNicoSeigaWebScraping変更通知プロパティ

        #region SeigaAccount変更通知プロパティ

        private NicovideoAccount _SeigaAccount;

        public NicovideoAccount SeigaAccount
        {
            get { return _SeigaAccount; }
            set
            {
                if (_SeigaAccount == value)
                    return;
                _SeigaAccount = value;
                RaisePropertyChanged();
            }
        }

        #endregion SeigaAccount変更通知プロパティ

        #endregion サイト別設定

        #region 外部アプリケーション

        #region DefaultBrowserPath変更通知プロパティ

        private string _DefaultBrowserPath;

        public string DefaultBrowserPath
        {
            get { return _DefaultBrowserPath; }
            set
            {
                if (_DefaultBrowserPath == value)
                    return;
                _DefaultBrowserPath = value;
                RaisePropertyChanged();
            }
        }

        #endregion DefaultBrowserPath変更通知プロパティ

        #endregion 外部アプリケーション

        #endregion 保持

        #region Version変更通知プロパティ

        private string _Version;

        public string Version
        {
            get { return _Version; }
            set
            {
                if (_Version == value)
                    return;
                _Version = value;
                RaisePropertyChanged();
            }
        }

        #endregion Version変更通知プロパティ

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

        #endregion OKCommand

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

        #endregion CancelCommand

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
            View.Focus();
            RaisePropertyChanged();

            Config.DefaultBrowserPath = File.Exists(DefaultBrowserPath) ? DefaultBrowserPath : null;
            Config.IsEnablePseudoSingleInstance = IsEnablePseudoSingleInstance;
            Config.IsEnableAggressiveMode = IsEnableAggressiveMode;
            Config.IsDisableNotificationWhenAggressiveMode = IsDisableNotificationWhenAggressiveMode;
            Config.IsKeepingTabsWhenAggressiveMode = IsKeepingTabsWhenAggressiveMode;
            Config.IsChildWindow = IsChildWindow;
            Config.IsFallbackTwitterGifMovie = IsFallbackTwitterGifMovie;
            Config.IsWarningTwitter30secMovie = IsWarningTwitter30secMovie;
            Config.IsUsePixivWebScraping = IsUsePixivWebScraping;
            Config.PixivAccount = PixivAccount;
            Config.IsUseNicoSeigaWebScraping = IsUseNicoSeigaWebScraping;
            Config.NicovideoAccount = SeigaAccount;
            Config.MouseSensibility = MouseSensibility;
        }

        #endregion ApplyCommand

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
            var message = new OpeningFileSelectionMessage("Open") {Filter = "実行可能ファイル(*.exe)|*.exe"};
            Messenger.Raise(message);
            if (message.Response == null)
            {
                return;
            }

            DefaultBrowserPath = message.Response[0];
        }

        #endregion SetDefaultBrowserCommand
    }
}