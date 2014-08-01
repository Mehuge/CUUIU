using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace UIU
{
    static class JSON
    {
        public static List<string> parse(string json)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<string>>(json);
        }

        public static string stringify(List<string> response)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(response);
        }
    }
}
