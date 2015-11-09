using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Infrastructures;
using ImageViewer.Models;

namespace ImageViewer.Helpers
{
    static class CookieHelper
    {
        private const string Ext = @".edat";

        public static CookieContainer LoadCookie(string service)
        {
            var filePath = Path.Combine(Config.AppPath, service + Ext);

            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                using (var encStream = File.Open(filePath, FileMode.Open))
                using (var rawStream = Encrypt.DecryptStream(encStream))
                {
                    var formatter = new BinaryFormatter();
                    return (CookieContainer)formatter.Deserialize(rawStream);
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool SaveCookie(CookieContainer cookie, string service)
        {
            var filePath = Path.Combine(Config.AppPath, service + Ext);

            try
            {
                using (var rawStream = File.Open(filePath, FileMode.Create))
                using (var encStream = Encrypt.EncryptStream(rawStream))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(encStream, cookie);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
