using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace ImageViewer.Models
{
    internal class JsonList : List<Json>
    {
        private readonly List<Json> _list;

        public JsonList()
        {
            _list = new List<Json>();
        }

        public JsonList(string json)
        {
            _list = new List<Json>();
            Parse(json);
        }

        public JsonList(JsonList jsonList)
        {
            _list = new List<Json>();
            _list.AddRange(jsonList);
        }

        private void Parse(string json)
        {
            var jsonRoot = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            jsonRoot.ToList().ForEach(x => _list.Add(new Json {Name = x.Key, Value = x.Value}));
        }
    }
}