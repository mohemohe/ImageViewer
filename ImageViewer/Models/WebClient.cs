using System;
using System.Net;

namespace ImageViewer.Models
{
    internal class WebClient : System.Net.WebClient
    {
        public int Timeout { get; set; } = 5000;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var req = base.GetWebRequest(uri);
            req.Timeout = Timeout;
            return req;
        }
    }
}