using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Nin.Aspnet;
using Nin.Diagnostic;

namespace Engine
{
    /// <summary>
    /// Functionality for controlling the service.
    /// </summary>
    public class ServiceController : ApiController
    {
        private readonly EngineProcess engine;

        public ServiceController(EngineProcess process)
        {
            engine = process;
        }

        public IActionResult Index()
        {
            return new NinHtmlResult("");
        }

        public IActionResult Stop()
        {
            Log.v(Tag, "Stopping service.");
            engine.Stop();
            return new NinHtmlResult("Stopping service...");
        }

        public IActionResult Start()
        {
            Log.v(Tag, "Starting service.");
            engine.Start();
            return new NinHtmlResult("Starting service...");
        }

        public IActionResult PostX(string y, string z)
        {
            return new NinHtmlResult(y +"_"+z);
        }

        const string Tag = "SCON";
    }
}
