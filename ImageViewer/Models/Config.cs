using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;

namespace ImageViewer.Models
{
    /// <summary>
    ///     XMLに書き出すための動的クラス
    /// </summary>
    public class Settings
    {
        [XmlElement(IsNullable = true)] public string DefaultBrowserPath;

        public bool? IsChildWindow;

        public bool? IsEnablePseudoSingleInstance;

        public bool? IsEnableAggressiveMode;

        public bool? IsDisableNotificationWhenAggressiveMode;

        public bool? IsKeepingTabsWhenAggressiveMode;

        public bool? IsFallbackTwitterGifMovie;

        public bool? IsUseNicoSeigaWebScraping;

        public bool? IsUsePixivWebScraping;

        // ReSharper disable once InconsistentNaming
        public bool? IsWarningTwitter30secMovie;

        public double? MouseSensibility;

        public NicovideoAccount NicovideoAccount;

        public PixivAccount PixivAccount;

        public Rect WindowPosition;
    }

    /// <summary>
    ///     設定を読み書きするクラス
    /// </summary>
    internal static class Config
    {
        private const string FileName = @"Settings.xml";
        public static readonly string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string FilePath = Path.Combine(AppPath, FileName);

        /// <summary>
        ///     ファイルから設定を読み込む
        /// </summary>
        public static void ReadConfig()
        {
            var xmlSettings = new Settings();
            var xs = new XmlSerializer(typeof (Settings));
            if (File.Exists(FilePath))
            {
                using (var fs = new FileStream(FilePath, FileMode.Open))
                using (var sr = new StreamReader(fs))
                using (var r = new StringReader(sr.ReadToEnd()))
                {
                    try
                    {
                        xmlSettings = (Settings) xs.Deserialize(r);
                    }
                    catch (InvalidOperationException e)
                    {
                        var ex = new InvalidOperationException("設定ファイルの読み込みに失敗しました。\nSettings.xml が不正な可能性があります。", e);
                        throw ex;
                    }
                        // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
            }

            _Config._DefaultBrowserPath = TryReadValue(xmlSettings.DefaultBrowserPath, null, null);
            _Config._WindowPosition = TryReadValue(xmlSettings.WindowPosition, null, null);
            _Config._MouseSensibility = TryReadValue(xmlSettings.MouseSensibility, null, 2.0);
            _Config._IsEnablePseudoSingleInstance = TryReadValue(xmlSettings.IsEnablePseudoSingleInstance, null, true);
            _Config._IsEnableAggressiveMode = TryReadValue(xmlSettings.IsEnableAggressiveMode, null, false);
            _Config._IsDisableNotificationWhenAggressiveMode = TryReadValue(xmlSettings.IsDisableNotificationWhenAggressiveMode, null, false);
            _Config._IsKeepingTabsWhenAggressiveMode = TryReadValue(xmlSettings.IsKeepingTabsWhenAggressiveMode, null, false);
            _Config._IsChildWindow = TryReadValue(xmlSettings.IsChildWindow, null, true);
            _Config._IsFallbackTwitterGifMovie = TryReadValue(xmlSettings.IsFallbackTwitterGifMovie, null, true);
            _Config._IsWarningTwitter30secMovie = TryReadValue(xmlSettings.IsWarningTwitter30secMovie, null, false);
            _Config._IsUsePixivWebScraping = TryReadValue(xmlSettings.IsUsePixivWebScraping, null, false);
            _Config._PixivAccount = TryReadValue(xmlSettings.PixivAccount, null, new PixivAccount());
            _Config._IsUseNicoSeigaWebScraping = TryReadValue(xmlSettings.IsUseNicoSeigaWebScraping, null, false);
            _Config._NicovideoAccount = TryReadValue(xmlSettings.NicovideoAccount, null, new NicovideoAccount());
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
            var backupFilePath = Path.Combine(AppPath, FileName + @".bak");

            var xmls = new Settings
            {
                DefaultBrowserPath = _Config._DefaultBrowserPath,
                WindowPosition = _Config._WindowPosition,
                MouseSensibility = _Config._MouseSensibility,
                IsEnablePseudoSingleInstance = _Config._IsEnablePseudoSingleInstance,
                IsEnableAggressiveMode = _Config._IsEnableAggressiveMode,
                IsDisableNotificationWhenAggressiveMode = _Config._IsDisableNotificationWhenAggressiveMode,
                IsKeepingTabsWhenAggressiveMode = _Config._IsKeepingTabsWhenAggressiveMode,
                IsChildWindow = _Config._IsChildWindow,
                IsFallbackTwitterGifMovie = _Config._IsFallbackTwitterGifMovie,
                IsWarningTwitter30secMovie = _Config._IsWarningTwitter30secMovie,
                IsUsePixivWebScraping = _Config._IsUsePixivWebScraping,
                PixivAccount = _Config._PixivAccount,
                IsUseNicoSeigaWebScraping = _Config._IsUseNicoSeigaWebScraping,
                NicovideoAccount = _Config._NicovideoAccount
            };

            if (File.Exists(FilePath))
            {
                File.Move(FilePath, backupFilePath);
            }

            var xs = new XmlSerializer(typeof (Settings));
            using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(fs, xmls);
            }

            if (File.Exists(FilePath))
            {
                File.Delete(backupFilePath);
            }
            else
            {
                File.Move(backupFilePath, FilePath);
            }
        }

        # region Memory

        /// <summary>
        ///     実際の設定値はここに記憶される
        /// </summary>
        // ReSharper disable InconsistentNaming
        private static class _Config
        {
            public static string _DefaultBrowserPath { get; set; }
            public static Rect _WindowPosition { get; set; }
            public static double _MouseSensibility { get; set; }
            public static bool _IsEnablePseudoSingleInstance { get; set; }
            public static bool _IsEnableAggressiveMode { get; set; }
            public static bool _IsDisableNotificationWhenAggressiveMode { get; set; }
            public static bool _IsKeepingTabsWhenAggressiveMode { get; set; }
            public static bool _IsChildWindow { get; set; }
            public static bool _IsFallbackTwitterGifMovie { get; set; }
            public static bool _IsWarningTwitter30secMovie { get; set; }
            public static bool _IsUsePixivWebScraping { get; set; }
            public static PixivAccount _PixivAccount { get; set; }
            public static bool _IsUseNicoSeigaWebScraping { get; set; }
            public static NicovideoAccount _NicovideoAccount { get; set; }
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region Accessor

        public static string DefaultBrowserPath
        {
            get { return _Config._DefaultBrowserPath; }
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

        public static bool IsEnableAggressiveMode
        {
            get { return _Config._IsEnableAggressiveMode; }
            set { _Config._IsEnableAggressiveMode = value; }
        }

        public static bool IsDisableNotificationWhenAggressiveMode
        {
            get { return _Config._IsDisableNotificationWhenAggressiveMode; }
            set { _Config._IsDisableNotificationWhenAggressiveMode = value; }
        }

        public static bool IsKeepingTabsWhenAggressiveMode
        {
            get { return _Config._IsKeepingTabsWhenAggressiveMode; }
            set { _Config._IsKeepingTabsWhenAggressiveMode = value; }
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

        // ReSharper disable once InconsistentNaming
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

        public static bool IsUseNicoSeigaWebScraping
        {
            get { return _Config._IsUseNicoSeigaWebScraping; }
            set { _Config._IsUseNicoSeigaWebScraping = value; }
        }

        public static NicovideoAccount NicovideoAccount
        {
            get { return _Config._NicovideoAccount; }
            set { _Config._NicovideoAccount = value; }
        }

        #endregion Accessor
    }
}