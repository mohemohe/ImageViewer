using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;
using System;
using System.Windows;

namespace ImageViewer.ViewModels
{
    public class CaptionBarViewModel : ViewModel
    {
        public void Initialize()
        {
            Application.Current.MainWindow.StateChanged += MainWindow_StateChanged;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window != null)
            {
                WindowState = window.WindowState;
            }
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

        private static class ButtonState
        {
            internal static string Maximize
            {
                get { return "1"; }
            }

            internal static string Normal
            {
                get { return "2"; }
            }
        }

        #region CloseCommand

        private ViewModelCommand _CloseCommand;

        public ViewModelCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new ViewModelCommand(Close);
                }
                return _CloseCommand;
            }
        }

        public void Close()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "WindowMessage"));
        }

        #endregion CloseCommand

        #region MinimizeCommand

        private ViewModelCommand _MinimizeCommand;

        public ViewModelCommand MinimizeCommand
        {
            get
            {
                if (_MinimizeCommand == null)
                {
                    _MinimizeCommand = new ViewModelCommand(Minimize);
                }
                return _MinimizeCommand;
            }
        }

        public void Minimize()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Minimize, "WindowMessage"));
        }

        #endregion MinimizeCommand

        #region MaximizeCommand

        private ViewModelCommand _MaximizeCommand;

        public ViewModelCommand MaximizeCommand
        {
            get
            {
                if (_MaximizeCommand == null)
                {
                    _MaximizeCommand = new ViewModelCommand(Maximize);
                }
                return _MaximizeCommand;
            }
        }

        public void Maximize()
        {
            if (WindowState == WindowState.Normal)
            {
                Messenger.Raise(new WindowActionMessage(WindowAction.Maximize, "WindowMessage"));
            }
            else if (WindowState == WindowState.Maximized)
            {
                Messenger.Raise(new WindowActionMessage(WindowAction.Normal, "WindowMessage"));
            }
        }

        #endregion MaximizeCommand

        #region WindowState変更通知プロパティ

        private WindowState _WindowState;

        public WindowState WindowState
        {
            get { return _WindowState; }
            set
            {
                if (_WindowState == value)
                {
                    return;
                }
                _WindowState = value;

                if (value == WindowState.Normal)
                {
                    MaximizeButtonContent = ButtonState.Maximize;
                }
                if (value == WindowState.Maximized)
                {
                    MaximizeButtonContent = ButtonState.Normal;
                }

                RaisePropertyChanged();
            }
        }

        #endregion WindowState変更通知プロパティ

        #region MaximizeButtonContent変更通知プロパティ

        private string _MaximizeButtonContent = ButtonState.Maximize;

        public string MaximizeButtonContent
        {
            get { return _MaximizeButtonContent; }
            set
            {
                if (_MaximizeButtonContent == value)
                {
                    return;
                }
                _MaximizeButtonContent = value;
                RaisePropertyChanged();
            }
        }

        #endregion MaximizeButtonContent変更通知プロパティ
    }
}