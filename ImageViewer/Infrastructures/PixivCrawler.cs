using System;
using ImageViewer.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Helpers;
using HAP = HtmlAgilityPack;

namespace ImageViewer.Infrastructures
{
    public static class PixivCrawler
    {
        private static CookieContainer _cookie;
        private const string Service = @"Pixiv";

        private static async void Login()
        {
            const string loginUri = @"https://www.secure.pixiv.net/login.php";

            var id = Config.PixivAccount.Id;
            var password = Config.PixivAccount.RawPassword;

            var dic = new Dictionary<string, string>
            {
                [@"mode"] = @"login",
                [@"pixiv_id"] = id,
                [@"pass"] = password,
                [@"skip"] = @"1"
            };

            var param = "";
            dic.Keys.ToList().ForEach(x => param += x + @"=" + dic[x] + @"&");
            var data = Encoding.ASCII.GetBytes(param);

            _cookie = new CookieContainer();
            var req = (HttpWebRequest) WebRequest.Create(loginUri);
            req.Method = @"POST";
            req.ContentType = @"application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.CookieContainer = _cookie;
            using (var reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
            }

            using (var res = await req.GetResponseAsync())
            using (var resStream = res.GetResponseStream())
            {
                if (resStream != null)
                {
                    var enc = Encoding.GetEncoding(@"UTF-8");
                    using (var sr = new StreamReader(resStream, enc))
                    {
                        sr.ReadToEnd();
                    }
                }
            }

            CookieHelper.SaveCookie(_cookie, Service);
        }

        public static async Task<ImagePack> GetImage(string uri)
        {
            var result = new ImagePack();

            if (_cookie == null)
            {
                var cookie = CookieHelper.LoadCookie(Service);
                if (cookie != null)
                {
                    _cookie = cookie;
                }
            }

            var req = (HttpWebRequest) WebRequest.Create(uri);
            req.CookieContainer = _cookie;

            var getHtml = new Func<Task<string>>(async () => 
            {
                var encoder = Encoding.GetEncoding(@"UTF-8");
                using (var res = await req.GetResponseAsync())
                using (var resStream = res.GetResponseStream())
                {
                    if (resStream != null)
                    {
                        using (var sr = new StreamReader(resStream, encoder))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                    return string.Empty;
                }
            });

            var html = await getHtml();
            if (!html.Contains(@"ログアウト"))
            {
                Login();
                html = await getHtml();
            }

            var doc = new HAP.HtmlDocument
            {
                OptionCheckSyntax = false,
                OptionFixNestedTags = true
            };

            doc.LoadHtml(html);

            var imageUri =
                doc.DocumentNode.SelectSingleNode(@"//img[@class='original-image']")?
                    .GetAttributeValue(@"data-src", string.Empty) ??
                doc.DocumentNode.SelectSingleNode(@"//meta[@property='og:image']")?
                    .GetAttributeValue(@"content", string.Empty)?
                    .Replace(@"150x150", @"1200x1200");
            if (!string.IsNullOrEmpty(imageUri))
            {
                result.ImageUri = imageUri;

                req = (HttpWebRequest) WebRequest.Create(imageUri);
                req.CookieContainer = _cookie;
                req.Referer = uri;
                using (var res = await req.GetResponseAsync())
                using (var resStream = res.GetResponseStream())
                using (var ms = new MemoryStream())
                {
                    resStream?.CopyTo(ms);
                    result.ImageData = ms.ToArray();
                }
            }

            return result;
        }
    }
}