using Livet;
using Livet.Commands;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

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

        private void CalcZoom()
        {
            if (FallbackImage != null)
            {
                var imageSize = FallbackImage.Width*FallbackImage.Height;
                var renderSize = ImageRenderWidth*ImageRenderHeight;
                Zoom = Convert.ToInt32((renderSize/imageSize)*100);
            }
        }

        #region ImageUri変更通知プロパティ

        private string _ImageUri;

        public string ImageUri
        {
            get { return _ImageUri; }
            set
            {
                if (_ImageUri == value)
                    return;
                _ImageUri = value;
                RaisePropertyChanged();
            }
        }

        #endregion ImageUri変更通知プロパティ

        #region FallbackImage変更通知プロパティ

        private BitmapSource _FallbackImage;

        public BitmapSource FallbackImage
        {
            get { return _FallbackImage; }
            set
            {
                if (_FallbackImage == value)
                    return;
                _FallbackImage = value;
                RaisePropertyChanged();
                CalcZoom();
            }
        }

        #endregion FallbackImage変更通知プロパティ

        #region FallbackImageUri変更通知プロパティ

        private string _FallbackImageUri;

        public string FallbackImageUri
        {
            get { return _FallbackImageUri; }
            set
            {
                if (_FallbackImageUri == value)
                    return;
                _FallbackImageUri = value;
                RaisePropertyChanged();
            }
        }

        #endregion FallbackImageUri変更通知プロパティ

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
            var fileName = Path.GetFileNameWithoutExtension(FallbackImageUri);
            var ext = Path.GetExtension(FallbackImageUri);

            var tmpList = new List<string>
            {
                "JPEG(*.jpg;*.jpeg)|*.jpg;*.jpeg|",
                "Bitmap(*.bmp)|*.bmp|",
                "PNG(*.png)|*.png|",
                "GIF(*.gif)|*.gif|"
            };

            var filterList = new List<string>();
            filterList.AddRange(tmpList.Where(x => x.Contains(ext)));
            filterList.AddRange(tmpList.Where(x => !x.Contains(ext)));

            var filter = "";
            filterList.ForEach(x => filter += x);
            filter = filter.Remove(filter.Length - 1, 1);
            filter += "|All Files(*.*)|*.*";

            var message = new SavingFileSelectionMessage("Save")
            {
                AddExtension = true,
                FileName = Path.GetFileName(FallbackImageUri),
                Filter = filter
            };
            Messenger.Raise(message);
            if (message.Response == null)
            {
                return;
            }

            var targetFilePath = message.Response[0];

            var encoder =
                Path.GetExtension(targetFilePath) == ".jpeg"
                    ? new JpegBitmapEncoder()
                    : ext == ".jpg"
                        ? new JpegBitmapEncoder()
                        : ext == ".bmp"
                            ? new BmpBitmapEncoder()
                            : ext == ".png"
                                ? new PngBitmapEncoder()
                                : ext == ".gif"
                                    ? new GifBitmapEncoder()
                                    : (BitmapEncoder) (new PngBitmapEncoder());
            encoder.Frames.Add(BitmapFrame.Create(FallbackImage));

            using (var fs = new FileStream(targetFilePath, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }

        #endregion SaveImageCommand
    }
}