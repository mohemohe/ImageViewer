using Livet;

namespace ImageViewer.Helpers
{
    public class DimensionHelper
    {
        public class TwoDimension : NotificationObject
        {
            #region X変更通知プロパティ

            private double _X;

            public double X
            {
                get { return _X; }
                set
                {
                    if (_X == value)
                        return;
                    _X = value;
                    RaisePropertyChanged();
                }
            }

            #endregion X変更通知プロパティ

            #region Y変更通知プロパティ

            private double _Y;

            public double Y
            {
                get { return _Y; }
                set
                {
                    if (_Y == value)
                        return;
                    _Y = value;
                    RaisePropertyChanged();
                }
            }

            #endregion Y変更通知プロパティ
        }
    }
}