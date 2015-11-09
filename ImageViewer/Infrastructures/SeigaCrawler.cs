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
    public static class SeigaCrawler
    {
        private static CookieContainer _cookie;
        private const string Service = @"Nicovideo";
        private const string BaseUri = @"http://seiga.nicovideo.jp";
        private const string LohasUri = @"http://lohas.nicoseiga.jp";

        private static async Task Login()
        {
            const string loginUri = @"https://account.nicovideo.jp/api/v1/login";

            var enc = Encoding.GetEncoding(@"UTF-8");

            var id = Config.NicovideoAccount.Id;
            var password = System.Web.HttpUtility.UrlEncode(Config.NicovideoAccount.RawPassword, enc);

            var dic = new Dictionary<string, string>
            {
                [@"mail_tel"] = id,
                [@"password"] = password,
            };

            var param = "";
            dic.Keys.ToList().ForEach(x => param += x + @"=" + dic[x] + @"&");
            var data = Encoding.ASCII.GetBytes(param);

            _cookie = new CookieContainer();
            var req = (HttpWebRequest)WebRequest.Create(loginUri);
            req.Method = @"POST";
            req.ContentType = @"application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.CookieContainer = _cookie;
            using (var reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }

            using (var res = await req.GetResponseAsync())
            using (var resStream = res.GetResponseStream())
            {
                if (resStream != null)
                {
                    using (var sr = new StreamReader(resStream, enc))
                    {
                        var s = 
                        sr.ReadToEnd();
                    }
                }
            }

            CookieHelper.SaveCookie(_cookie, Service);
        }

        public static async Task<ImagePack> GetImage(string uri)
        {
            var result = new ImagePack();

            if (_cookie == null && Config.IsUseNicoSeigaWebScraping)
            {
                var cookie = CookieHelper.LoadCookie(Service);
                if (cookie != null)
                {
                    _cookie = cookie;
                }
            }

            var getHtml = new Func<string, Task<string>>(async (u) =>
            {
                var r = string.Empty;

                var req = (HttpWebRequest)WebRequest.Create(u);
                req.CookieContainer = _cookie;

                var encoder = Encoding.GetEncoding(@"UTF-8");
                using (var res = await req.GetResponseAsync())
                using (var resStream = res.GetResponseStream())
                {
                    if (resStream != null)
                    {
                        using (var sr = new StreamReader(resStream, encoder))
                        {
                            r = sr.ReadToEnd();
                        }
                    }
                }

                return r;
            });

            var html = await getHtml(uri);
            if (Config.IsUseNicoSeigaWebScraping && !html.Contains(@"ログアウト"))
            {
                await Login();
                html = await getHtml(uri);
            }
            if(Config.IsUseNicoSeigaWebScraping)
            {
                CookieHelper.SaveCookie(_cookie, Service);
            }

            var doc = new HAP.HtmlDocument
            {
                OptionCheckSyntax = false,
                OptionFixNestedTags = true
            };

            doc.LoadHtml(html);

            string imageUri;
            var fallbackUri = doc.DocumentNode.SelectSingleNode(@"//meta[@property='og:image']")?
                                              .GetAttributeValue(@"content", string.Empty);
            if (Config.IsUseNicoSeigaWebScraping)
            {
                var nextUri = doc.DocumentNode.SelectSingleNode(@"//a[@id='illust_link']")?
                    .GetAttributeValue(@"href", null);
                if (!string.IsNullOrEmpty(nextUri))
                {
                    html = await getHtml(BaseUri + nextUri);
                    doc.LoadHtml(html);

                    var origUri = doc.DocumentNode.SelectSingleNode(@"//div[@class='illust_view_big']")?
                        .ChildNodes?.Where(x => x.Name == @"img")?.ElementAt(0)?
                        .GetAttributeValue(@"src", string.Empty);
                    if (!string.IsNullOrEmpty(origUri))
                    {
                        imageUri = LohasUri + origUri;
                    }
                    else
                    {
                        imageUri = fallbackUri;
                    }
                }
                else
                {
                    imageUri = fallbackUri;
                }
            }
            else
            {
                imageUri = fallbackUri;
            }

            if (!string.IsNullOrEmpty(imageUri))
            {
                result.ImageUri = imageUri;

                var req = (HttpWebRequest) WebRequest.Create(imageUri);
                using (var res = await req.GetResponseAsync())
                using (var resStream = res.GetResponseStream())
                using (var ms = new MemoryStream())
                {
                    resStream?.CopyTo(ms);
                    result.ImageData = ms.ToArray();
                }
            }

            CookieHelper.SaveCookie(_cookie, Service);

            return result;
        }
    }
}