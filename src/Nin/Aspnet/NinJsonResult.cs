using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Nin.Aspnet
{
    public class NinJsonResult : ContentResult
    {
        public NinJsonResult(object content)
        {
            ContentType = "application/json";
            Content = Jsonify(content);
        }

        private string Jsonify(object content)
        {
            var s = content as string;
            if (s != null) return s;
            return JsonConvert.SerializeObject(content);
        }
    }
}