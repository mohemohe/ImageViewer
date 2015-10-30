using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace ImageViewer.Models
{
    /// <summary>
    ///     XMLに書き出すための動的クラス
    /// </summary>
    public class Settings
    {
        [XmlElement(IsNullable = true)]
        public string DefaultBrowserPath;

        public Rect WindowPosition;

        public double? MouseSensibility;

        public bool? IsEnablePseudoSingleInstance;

        public bool? IsChildWindow;

        public bool? IsFallbackTwitterGifMovie;

        public bool? IsWarningTwitter30secMovie;

        public bool? IsUsePixivWebScraping;

        [XmlElement(IsNullable = true)]
        public PixivAccount PixivAccount;
    }

    /// <summary>
    ///     設定を読み書きするクラス
    /// </summary>
    internal class Config
    {
        # region Memory

        /// <summary>
        ///     実際の設定値はここに記憶される
        /// </summary>
        protected class _Config
        {
            public static string _DefaultBrowserPath { get; set; }
            public static Rect _WindowPosition { get; set; }
            public static double _MouseSensibility { get; set; }
            public static bool _IsEnablePseudoSingleInstance { get; set; }
            public static bool _IsChildWindow { get; set; }
            public static bool _IsFallbackTwitterGifMovie { get; set; }
            public static bool _IsWarningTwitter30secMovie { get; set; }
            public static bool _IsUsePixivWebScraping { get; set; }
            public static bool _IsGetPixivR18Images { get; set; }
            public static PixivAccount _PixivAccount { get; set; }
        }

        #endregion

        #region Accessor

        public static string DefaultBrowserPath
        {
            get { return  _Config._DefaultBrowserPath; }
            set { _Config._DefaultBrowserPath = value; }
        }

        public static Rect WindowPosition
        {
            get { return _Config._WindowPosition; }
            set { _Config._WindowPosition = value; }
        }

        public static double MouseSensibility
        {
            get { return _Config._MouseSensibility; }
            set { _Config._MouseSensibility = value; }
        }

        public static bool IsEnablePseudoSingleInstance
        {
            get { return _Config._IsEnablePseudoSingleInstance; }
            set { _Config._IsEnablePseudoSingleInstance = value; }
        }

        public static bool IsChildWindow
        {
            get { return _Config._IsChildWindow; }
            set { _Config._IsChildWindow = value; }
        }

        public static bool IsFallbackTwitterGifMovie
        {
            get { return _Config._IsFallbackTwitterGifMovie; }
            set { _Config._IsFallbackTwitterGifMovie = value; }
        }
        public static bool IsWarningTwitter30secMovie
        {
            get { return _Config._IsWarningTwitter30secMovie; }
            set { _Config._IsWarningTwitter30secMovie = value; }
        }

        public static bool IsUsePixivWebScraping
        {
            get { return _Config._IsUsePixivWebScraping; }
            set { _Config._IsUsePixivWebScraping = value; }
        }

        public static PixivAccount PixivAccount
        {
            get { return _Config._PixivAccount; }
            set { _Config._PixivAccount = value; }
        }

        #endregion Accessor

        private static readonly string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string fileName = @"Settings.xml";
        private static readonly string filePath = Path.Combine(appPath, fileName);

        /// <summary>
        ///     ファイルから設定を読み込む
        /// </summary>
        public static void ReadConfig()
        {
            var xmlSettings = new Settings();
            var xs = new XmlSerializer(typeof(Settings));
            if (File.Exists(filePath))
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    xmlSettings = (Settings)xs.Deserialize(fs);
                    fs.Close();
                }
            }

            _Config._DefaultBrowserPath = TryReadValue(xmlSettings.DefaultBrowserPath, null, null);
            _Config._WindowPosition = TryReadValue(xmlSettings.WindowPosition, null, null);
            _Config._MouseSensibility = TryReadValue(xmlSettings.MouseSensibility, null, 2.0);
            _Config._IsEnablePseudoSingleInstance = TryReadValue(xmlSettings.IsEnablePseudoSingleInstance, null, true);
            _Config._IsChildWindow = TryReadValue(xmlSettings.IsChildWindow, null, true);
            _Config._IsFallbackTwitterGifMovie = TryReadValue(xmlSettings.IsFallbackTwitterGifMovie, null, true);
            _Config._IsWarningTwitter30secMovie = TryReadValue(xmlSettings.IsWarningTwitter30secMovie, null, false);
            _Config._IsUsePixivWebScraping = TryReadValue(xmlSettings.IsUsePixivWebScraping, null, false);
            _Config._PixivAccount = TryReadValue(xmlSettings.PixivAccount, null, new PixivAccount());
        }

        private static dynamic TryReadValue(dynamic source, dynamic check, dynamic defaultValue)
        {
            if (source != check)
            {
                return source;
            }
            return defaultValue;
        }

        /// <summary>
        ///     ファイルへ設定を書き込む
        /// </summary>
        public static void WriteConfig()
        {
            var xmls = new Settings
            {
                DefaultBrowserPath = _Config._DefaultBrowserPath,
                WindowPosition = _Config._WindowPosition,
                MouseSensibility = _Config._MouseSensibility,
                IsEnablePseudoSingleInstance = _Config._IsEnablePseudoSingleInstance,
                IsChildWindow = _Config._IsChildWindow,
                IsFallbackTwitterGifMovie = _Config._IsFallbackTwitterGifMovie,
                IsWarningTwitter30secMovie = _Config._IsWarningTwitter30secMovie,
                IsUsePixivWebScraping = _Config._IsUsePixivWebScraping,
                PixivAccount = _Config._PixivAccount,
            };

            var xs = new XmlSerializer(typeof(Settings));
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(fs, xmls);
            }
        }
    }
}
