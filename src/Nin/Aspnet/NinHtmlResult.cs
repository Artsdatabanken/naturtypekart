using Microsoft.AspNetCore.Mvc;

namespace Nin.Aspnet
{
    public class NinHtmlResult : ContentResult
    {
        public NinHtmlResult(string content)
        {
            Content = content;
            ContentType = "text/html";
        }
    }
}