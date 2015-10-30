using System;
using System.Net;

namespace ImageViewer.Infrastructures
{
    internal class WebClient : System.Net.WebClient
    {
        public int Timeout { private get; set; } = 5000;
        public string Referer { private get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = (HttpWebRequest) base.GetWebRequest(address);
            if (req != null)
            {
                req.Timeout = Timeout;
                req.Referer = Referer;
            }
            return req;
        }
    }
}