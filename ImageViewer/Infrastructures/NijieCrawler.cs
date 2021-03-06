﻿using ImageViewer.Models;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HAP = HtmlAgilityPack;

namespace ImageViewer.Infrastructures
{
    public static class NijieCrawler
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

            var imageUri = doc.DocumentNode.SelectSingleNode(@"//meta[@property='og:image']")?
                .GetAttributeValue(@"content", string.Empty)?
                .Replace(@"/sp/", @"/");
            if (!string.IsNullOrEmpty(imageUri))
            {
                result.ImageUri = imageUri;

                req = (HttpWebRequest) WebRequest.Create(imageUri);
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