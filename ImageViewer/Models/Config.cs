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

        public bool? IsChildWindow;
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
            public static bool _IsChildWindow { get; set; }
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

        public static bool IsChildWindow
        {
            get { return _Config._IsChildWindow; }
            set { _Config._IsChildWindow = value; }
        }

        #endregion Accessor

        private static readonly string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string fileName = "Settings.xml";
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
            _Config._IsChildWindow = TryReadValue(xmlSettings.IsChildWindow, null, true);
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
                IsChildWindow = _Config._IsChildWindow,
            };

            var xs = new XmlSerializer(typeof(Settings));
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(fs, xmls);
            }
        }
    }
}
