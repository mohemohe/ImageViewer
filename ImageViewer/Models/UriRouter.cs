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

        public static bool IsImageUri(string uri, out string imageUri)
        {
            var result = false;
            string targetUri = null;
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
                string twippleResult;
                if (IsTwipplePhoto(uri, out twippleResult))
                {
                    targetUri = twippleResult;
                    result = true;
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

        private static bool IsTwipplePhoto(string uri, out string resultUri)
        {
            var result = false;
            resultUri = null;
            var regex = new Regex(@"(?<baseUri>(http://|https://)(p.twipple.jp/|p.twpl.jp/))(show/)?(thumb/|large/|orig/)?(?<imageId>.*)");
            if (regex.IsMatch(uri))
            {
                var match = regex.Matches(uri);
                result = true;
                resultUri = match[0].Groups["baseUri"] + @"show/orig/" + match[0].Groups["imageId"];
            }

            return result;
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
                    using (var sr = new StreamReader(resStream, Encoding.UTF8))
                    {
                        var resBody = sr.ReadToEnd();
                        var resJson = new JsonList(resBody);
                        resultUri =
                            resJson.Where(x => x.Name.Contains("full")).Where(x => x.Value != null).ToList().ToString();
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