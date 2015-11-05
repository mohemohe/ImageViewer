using ImageViewer.Models;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ImageViewer.Infrastructures
{
    public static class UriRouter
    {
        private static List<string> _IsImageList;

        private static readonly List<string> BlackList = new List<string>
        {
            @"http(s)?://(www.)?1drv.ms/.*",
            @"http(s)?://(www.)?(nicovideo.jp|nico.ms)/.*",
            @"http(s)?://(www.)?(ustre.am|ustream.tv)/.*",
            @"http(s)?://(www.)?vine.co/.*",
            @"http(s)?://(www.)?vimeo.com/.*",
            @"http(s)?://(www.)?(youtube.com|youtu.be)/.*"
        };

        private static readonly List<FuncDelegate<string, string, bool>> WhiteList = new List
            <FuncDelegate<string, string, bool>>
        {
            IsTwipplePhoto,
            IsInstagramPhoto,
            IsGyazoPhoto,
            IsGamenNowPhoto,
            IsPixivPhoto,
            IsNijiePhoto
        };

        private static List<string> IsImageList
        {
            get
            {
                if (_IsImageList == null)
                {
                    _IsImageList = new List<string>
                    {
                        @":orig",
                        @":large"
                    };

                    var decoders = ImageCodecInfo.GetImageDecoders();
                    foreach (var ici in decoders)
                    {
                        _IsImageList.AddRange(ici.FilenameExtension.Replace(@"*", @"").ToLower().Split(';'));
                    }
                }

                return _IsImageList;
            }
        }

        public static bool IsImageUri(ref string uri, out string imageUri)
        {
            var result = false;
            var queryUri = uri;
            string targetUri = null;

            if (IsBlackListedUri(uri))
            {
                imageUri = null;
                return result;
            }

            if (Config.IsFallbackTwitterGifMovie && IsVideoThumbUri(queryUri))
            {
                var regex =
                    new Regex(
                        @"(?<baseUri>http(s)?://pbs.twimg.com/)(?<thumb>tweet_video_thumb)(?<mediaId>.*)(?<ext>\..*)(?<size>:(orig|large))?");
                if (regex.IsMatch(uri))
                {
                    var match = regex.Matches(uri);

                    // .mp4 決め打ちでいいんだろうか
                    // GIFはmp4に変換してるからいいとは思うんだけど
                    uri = match[0].Groups[@"baseUri"] + @"tweet_video" + match[0].Groups[@"mediaId"] + @".mp4";
                }
                imageUri = null;
                return result;
            }

            IsImageList.ForEach(x =>
            {
                if (queryUri.ToLower().EndsWith(x))
                {
                    targetUri = queryUri;
                    result = true;
                }
            });

            if (result == false)
            {
                foreach (var func in WhiteList)
                {
                    if (func(queryUri, out targetUri))
                    {
                        result = true;
                        break;
                    }
                }
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

        private static bool IsVideoThumbUri(string uri)
        {
            var regex = new Regex(@"http(s)?://pbs.twimg.com/tweet_video_thumb/.*");
            return regex.IsMatch(uri);
        }

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
                    {
                        if (resStream != null)
                        {
                            using (var sr = new StreamReader(resStream, Encoding.UTF8))
                            {
                                var resBody = sr.ReadToEnd();
                                var resJson = new JsonList(resBody);
                                resultUri =
                                    resJson.Where(x => x.Name.Contains(@"full")).Where(x => x.Value != null).ToList()[0]
                                        ?
                                        .Value
                                        .ToString();
                                return true;
                            }
                        }
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

        private delegate V FuncDelegate<T, U, V>(T uri, out U imageUri);

        #region WhiteListMethods

        private static bool IsTwipplePhoto(string uri, out string resultUri)
        {
            var result = false;
            resultUri = null;
            var regex =
                new Regex(
                    @"(?<baseUri>http(s)?://(p.twipple.jp|p.twpl.jp)/)(show/)?(thumb/|large/|orig/)?(?<imageId>.*)");
            if (regex.IsMatch(uri))
            {
                var match = regex.Matches(uri);
                result = true;
                resultUri = match[0].Groups[@"baseUri"] + @"show/orig/" + match[0].Groups[@"imageId"];
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

            resultUri = null;
            var regex = new Regex(@"(?<baseUri>http(s)?://gyazo.com/)(?<imageId>.*)");
            if (regex.IsMatch(uri))
            {
                var req = WebRequest.Create(apiBaseUri + uri);
                req.Timeout = 3000;

                try
                {
                    var res = (HttpWebResponse) req.GetResponse();
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        using (var resStream = res.GetResponseStream())
                        {
                            if (resStream != null)
                            {
                                using (var sr = new StreamReader(resStream, Encoding.UTF8))
                                {
                                    var resBody = sr.ReadToEnd();
                                    var resJson = new JsonList(resBody);
                                    resultUri =
                                        resJson.Where(x => x.Name == "url").Where(x => x.Value != null).ToList()[0]?
                                            .Value
                                            .ToString();
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
                catch
                {
                    resultUri = null;
                    return false;
                }
            }

            return false;
        }

        private static bool IsGamenNowPhoto(string uri, out string resultUri)
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

        private static bool IsPixivPhoto(string uri, out string resultUri)
        {
            var result = false;
            resultUri = null;

            if (Config.IsUsePixivWebScraping)
            {
                var regex = new Regex(@"(?<baseUri>http(s)?://(www.)?pixiv.net/member_illust.php)(?<args>.*)");
                if (regex.IsMatch(uri))
                {
                    result = true;
                    resultUri = @"{Pixiv}";
                }
            }
            return result;
        }

        private static bool IsNijiePhoto(string uri, out string resultUri)
        {
            var result = false;
            resultUri = null;

            var regex = new Regex(@"(?<baseUri>http(s)?://(www.)?nijie.info/view.php)(?<args>.*)");
            if (regex.IsMatch(uri))
            {
                result = true;
                resultUri = @"{Nijie}";
            }
            return result;
        }

        #endregion WhiteListMethods
    }
}