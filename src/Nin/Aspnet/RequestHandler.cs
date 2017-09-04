using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Nin.Aspnet
{
    public class RequestHandler
    {
        private readonly MethodInfo methodInfo;
        private readonly object[] args;
        private readonly object controller;

        public RequestHandler(object controller, MethodInfo mi, object[] args)
        {
            this.controller = controller;
            this.args = args;
            methodInfo = mi;
        }

        public Task RequestDelegate(HttpContext context)
        {
            object returnValue = methodInfo.Invoke(controller, args);
            var rv = returnValue as IActionResult;
            if (rv != null)
            {
                IActionResult ar = rv;
                ActionContext acontext = new ActionContext();
                return ar.ExecuteResultAsync(acontext);
            }
            var response = context.Response;
            //response.Headers.Add("Access-Control-Allowed-Origins", "*");
            response.ContentType = "application/json";
            WriteResponse(returnValue, response);
            return Task.FromResult(0);
        }

        private static void WriteResponse(object returnValue, HttpResponse response)
        {
            var s = returnValue as string;
            if (s != null)
            {
                response.WriteAsync(s);
                return;
            }
            using (var streamWriter = new StreamWriter(response.Body))
            {
                var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings());
                serializer.Serialize(streamWriter, returnValue);
                streamWriter.Flush();
            }
        }
    }
}
