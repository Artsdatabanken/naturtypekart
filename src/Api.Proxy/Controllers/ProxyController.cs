using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Nin.Configuration;
using Nin.Diagnostic;
using Raven.Abstractions.Exceptions;

namespace Api.Proxy.Controllers
{
    public class HomeController : ApiController
    {
        /// <summary>
        /// Proxy a remote resource.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<ActionResult> Index(string url)
        {
            if (string.IsNullOrEmpty(url)) return Status();
            url = WebUtility.UrlDecode(url);
            CheckForAllowedUrl(url);
            using (var client = new HttpClient())
            {
                Log.v("PROX", "Download " + url);
                using (var response = await client.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.w("PROX", "Download " + url + " failed: " + response);
                        return new StatusCodeResult((int)response.StatusCode);
                    }
                    var httpContent = response.Content;
                    var content = await httpContent.ReadAsByteArrayAsync();
                    var contentType = GetContentType(httpContent);
                    return new FileContentResult(content, contentType);
                }
            }
        }

        private ActionResult Status()
        {
            return Ok("Running fine...");
        }

        private static string GetContentType(HttpContent httpContent)
        {
            string contentType = "text/html";
            IEnumerable<string> ctHeaders;
            if (httpContent.Headers.TryGetValues("ContentType", out ctHeaders))
                contentType = ctHeaders.First();
            return contentType;
        }

        private void CheckForAllowedUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new BadRequestException();
            foreach (var validPrefix in Config.Settings.Proxy.AllowedUrlPrefixes)
                if (url.StartsWith(validPrefix, StringComparison.OrdinalIgnoreCase)) return;
            var connection = (Microsoft.AspNetCore.Http.DefaultHttpContext)Request.Properties["HttpContext"];
            Log.w("PROX", $"Unauthorized access attempt from { connection.Connection.RemoteIpAddress} URL: {url}");
            throw new UnauthorizedAccessException(url +" is not on allowed.");
        }
    }
}
