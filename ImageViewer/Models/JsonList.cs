using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Script.Serialization;

namespace ImageViewer.Models
{
    internal class JsonList : Collection<Json>
    {
        public JsonList()
        {
        }

        public JsonList(string json)
        {
            Parse(json);
        }

        public JsonList(JsonList jsonList)
        {
            jsonList.ToList().ForEach(Add);
        }

        private void Parse(string json)
        {
            var jsonRoot = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            jsonRoot.ToList().ForEach(x => Add(new Json {Name = x.Key, Value = x.Value}));
        }
    }
}