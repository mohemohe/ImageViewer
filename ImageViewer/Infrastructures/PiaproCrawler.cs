using ImageViewer.Models;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HAP = HtmlAgilityPack;

namespace ImageViewer.Infrastructures
{
    public static class PiaproCrawler
    {
        public static async Task<ImagePack> GetImage(string uri)
        {
            var result = new ImagePack();

            var req = (HttpWebRequest) WebRequest.Create(uri);

            var html = "";

            var encoder = Encoding.GetEncoding(@"UTF-8");
            using (var res = await req.GetResponseAsync())
            using (var resStream = res.GetResponseStream())
                if (resStream != null)
                {
                    using (var sr = new StreamReader(resStream, encoder))
                    {
                        html = sr.ReadToEnd();
                    }
                }

            var doc = new HAP.HtmlDocument
            {
                OptionCheckSyntax = false,
                OptionFixNestedTags = true
            };

            doc.LoadHtml(html);

            var divStyle = doc.DocumentNode.SelectSingleNode(@"//div[@id='_image']")?.GetAttributeValue(@"style", null);
            if (!string.IsNullOrEmpty(divStyle))
            {
                var regex = new Regex(@"background:url\((.*)\).*");
                if (regex.IsMatch(divStyle))
                {
                    var imageUri = regex.Matches(divStyle)[0].Groups[1].Value;
                    result.ImageUri = imageUri;

                    req = (HttpWebRequest)WebRequest.Create(imageUri);
                    using (var res = await req.GetResponseAsync())
                    using (var resStream = res.GetResponseStream())
                    using (var ms = new MemoryStream())
                    {
                        resStream?.CopyTo(ms);
                        result.ImageData = ms.ToArray();
                    }
                }
            }

            return result;
        }
    }
}