using ImageViewer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HAP = HtmlAgilityPack;

namespace ImageViewer.Infrastructures
{
    public static class PixivCrawler
    {
        static CookieContainer cookie = null;

        public static bool Login()
        {
            const string loginUri = @"https://www.secure.pixiv.net/login.php";

            var id = Config.PixivAccount.Id;
            var password = Config.PixivAccount.RawPassword;

            var dic = new Dictionary<string, string>();
            dic[@"mode"] = @"login";
            dic[@"pixiv_id"] = id;
            dic[@"pass"] = password;
            dic[@"skip"] = "1";

            string param = "";
            dic.Keys.ToList().ForEach(x => param += x + @"=" + dic[x] + @"&");
            var data = Encoding.ASCII.GetBytes(param);

            cookie = new CookieContainer();
            var req = (HttpWebRequest)WebRequest.Create(loginUri);
            req.Method = @"POST";
            req.ContentType = @"application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.CookieContainer = cookie;
            using (var reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
            }

            using (var res = req.GetResponse())
            using (var resStream = res.GetResponseStream())
            {
                var enc = Encoding.GetEncoding(@"UTF-8");
                using (var sr = new StreamReader(resStream, enc))
                {
                    var html = sr.ReadToEnd();
                    return html.Contains(@"ログアウト");
                }
            }
        }

        public class PixivImageInfo
        {
            public string ImageUri { get; set; }
            public byte[] ImageData { get; set; }
        }

        public static async Task<PixivImageInfo> GetImage(string uri)
        {
            var result = new PixivImageInfo();

            if (cookie == null)
            {
                Login();
            }

            var req = (HttpWebRequest)WebRequest.Create(uri);
            req.CookieContainer = cookie;

            string html;

            var encoder = Encoding.GetEncoding("UTF-8");
            using (var res = await req.GetResponseAsync())
            using (var resStream = res.GetResponseStream())
            using (var sr = new StreamReader(resStream, encoder))
            {
                html = sr.ReadToEnd();
            }

            var doc = new HAP.HtmlDocument();
            doc.OptionCheckSyntax = false;
            doc.OptionFixNestedTags = true;

            doc.LoadHtml(html);

            var imageUri = doc.DocumentNode.SelectSingleNode(@"//img[@class='original-image']")?.GetAttributeValue(@"data-src", string.Empty);
            if (!string.IsNullOrEmpty(imageUri))
            {
                result.ImageUri = imageUri;

                req = (HttpWebRequest)WebRequest.Create(imageUri);
                req.CookieContainer = cookie;
                req.Referer = uri;
                using (var res = await req.GetResponseAsync())
                using (var resStream = res.GetResponseStream())
                using (var ms = new MemoryStream())
                {
                    resStream.CopyTo(ms);
                    result.ImageData = ms.ToArray();
                }
            }

            return result;
        }
    }
}
