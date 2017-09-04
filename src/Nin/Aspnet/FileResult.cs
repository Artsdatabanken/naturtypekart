using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nin.Aspnet
{
    public class DownloadFileResult : ActionResult
    {
        public DownloadFileResult(string fileDownloadName, Stream content, string contentType)
        {
            FileDownloadName = fileDownloadName;
            Content = content;
            ContentType = contentType;
        }

        public string ContentType { get; private set; }
        public string FileDownloadName { get; private set; }
        public Stream Content { get; private set; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null) throw new Exception("context is null");
            if (context.HttpContext == null) throw new Exception("context.HttpContext is null");
            if (context.HttpContext.Response == null) throw new Exception("context.HttpContext.Response is null");
            if (context.HttpContext.Response.Headers == null) throw new Exception("context.HttpContext.Response.Headers is null");
            var response = context.HttpContext.Response;
            response.ContentType = ContentType;
            response.Headers.Add("Content-Disposition", new[] { "attachment; filename=" + FileDownloadName });
            await Content.CopyToAsync(response.Body);
        }
    }
}
