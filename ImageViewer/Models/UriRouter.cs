using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ImageViewer.Models
{
    internal static class UriRouter
    {
        private static readonly List<string> IsImageList = new List<string>
        {
            ":orig",
            ".jpeg",
            ".jpg",
            ".bmp",
            ".png",
            ".gif"
        };

        private static readonly List<string> BlackList = new List<string>
        {
            @"http(s)?://(www.)?1drv.ms/.*",
            @"http(s)?://(www.)?(youtube.com|youtu.be)/.*",
            @"http(s)?://(www.)?(nicovideo.jp|nico.ms)/.*",
            @"http(s)?://(www.)?vine.co/.*",
            @"http(s)?://(www.)?vimeo.com/.*",
            @"http(s)?://(www.)?(ustre.am|ustream.tv)/.*",
        };

        delegate V FuncDelegate<T, U, V>(T uri, out U imageUri);
        private static readonly List<FuncDelegate<string, string, bool>> WhiteList = new List<FuncDelegate<string, string, bool>>
        {
            new FuncDelegate<string, string, bool>(IsTwipplePhoto),
            new FuncDelegate<string, string, bool>(IsInstagramPhoto),
            new FuncDelegate<string, string, bool>(IsGyazoPhoto),
            new FuncDelegate<string, string, bool>(IsGamenNow),
        };

        public static bool IsImageUri(string uri, out string imageUri)
        {
            var result = false;
            string targetUri = null;

            if (IsBlackListedUri(uri))
            {
                imageUri = null;
                return result;
            }

            IsImageList.ForEach(x =>
            {
                if (uri.EndsWith(x))
                {
                    targetUri = uri;
                    result = true;
                }
            });

            if (result == false)
            {
                WhiteList.ForEach(x => result = x(uri, out targetUri));
            }

            if (result == false)
            {
                string apiResult;
                if (GetAzyobuziApiResult(uri, out apiResult))
                {
                    targetUri = apiResult;
                    result = true;
                }
            }

            imageUri = targetUri;
            return result;
        }

        private static bool IsBlackListedUri(string uri)
        {
            var result = false;
            BlackList.ForEach(x =>
            {
                var regex = new Regex(x);
                if (regex.IsMatch(uri))
                {
                    result = true;
                }
            });

            return result;
        }

        #region WhiteListMethods
        private static bool IsTwipplePhoto(string uri, out string resultUri)
        {
            var result = false;
            resultUri = null;
            var regex = new Regex(@"(?<baseUri>http(s)?://(p.twipple.jp|p.twpl.jp)/)(show/)?(thumb/|large/|orig/)?(?<imageId>.*)");
            if (regex.IsMatch(uri))
            {
                var match = regex.Matches(uri);
                result = true;
                resultUri = match[0].Groups["baseUri"] + @"show/orig/" + match[0].Groups["imageId"];
            }

            return result;
        }

        private static bool IsInstagramPhoto(string uri, out string resultUri)
        {
            var result = false;
            resultUri = null;
            var regex = new Regex(@"(?<baseUri>http(s)?://instagram.com/p/)(?<imageId>.*)/");
            if (regex.IsMatch(uri))
            {
                result = true;
                resultUri = uri + @"media/?size=l";
            }

            return result;
        }

        private static bool IsGyazoPhoto(string uri, out string resultUri)
        {
            const string apiBaseUri = @"https://api.gyazo.com/api/oembed/?url=";

            var result = false;
            resultUri = null;
            var regex = new Regex(@"(?<baseUri>http(s)?://gyazo.com/)(?<imageId>.*)");
            if (regex.IsMatch(uri))
            {
                result = true;

                var req = WebRequest.Create(apiBaseUri + uri);
                req.Timeout = 3000;

                try
                {
                    var res = (HttpWebResponse)req.GetResponse();
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        using (var resStream = res.GetResponseStream())
                        using (var sr = new StreamReader(resStream, Encoding.UTF8))
                        {
                            var resBody = sr.ReadToEnd();
                            var resJson = new JsonList(resBody);
                            resultUri =
                                resJson.Where(x => x.Name == "url").Where(x => x.Value != null).ToList()[0]?.Value.ToString();
                            return true;
                        }
                    }
                    resultUri = null;
                    return false;
                }
                catch
                {
                    resultUri = null;
                    return false;
                }
            }

            return result;
        }

        private static bool IsGamenNow(string uri, out string resultUri)
        {
            var result = false;
            resultUri = null;
            var regex = new Regex(@"(?<baseUri>http(s)?://s.kuku.lu/)(?<imageId>.*)");
            if (regex.IsMatch(uri))
            {
                var match = regex.Matches(uri);
                result = true;
                resultUri = match[0].Groups["baseUri"] + @"image.php/" + match[0].Groups["imageId"];
            }

            return result;
        }
        #endregion WhiteListMethods

        private static bool GetAzyobuziApiResult(string uri, out string resultUri)
        {
            const string apiBaseUri = @"http://img.azyobuzi.net/api/all_sizes.json?uri=";

            var req = WebRequest.Create(apiBaseUri + uri);
            req.Timeout = 3000;

            try
            {
                var res = (HttpWebResponse) req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (var resStream = res.GetResponseStream())
                    using (var sr = new StreamReader(resStream, Encoding.UTF8))
                    {
                        var resBody = sr.ReadToEnd();
                        var resJson = new JsonList(resBody);
                        resultUri =
                            resJson.Where(x => x.Name.Contains("full")).Where(x => x.Value != null).ToList()[0]?.Value.ToString();
                        return true;
                    }
                }
                resultUri = null;
                return false;
            }
            catch
            {
                resultUri = null;
                return false;
            }
        }
    }
}