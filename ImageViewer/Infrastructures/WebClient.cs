using System;
using System.Net;

namespace ImageViewer.Infrastructures
{
    internal class WebClient : System.Net.WebClient
    {
        public int Timeout { get; set; } = 5000;
        public string Referer { get; set; } = null;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var req = (HttpWebRequest)base.GetWebRequest(uri);
            req.Timeout = Timeout;
            req.Referer = Referer?.ToString();
            return req;
        }
    }
}