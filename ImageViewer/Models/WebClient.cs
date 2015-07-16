using System;
using System.Net;

namespace ImageViewer.Models
{
    internal class WebClient : System.Net.WebClient
    {
        private int _Timeout = 5000;

        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var w = base.GetWebRequest(uri);
            w.Timeout = _Timeout;
            return w;
        }
    }
}