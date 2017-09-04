using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using Nin.Diagnostic;

namespace Engine
{
    public static class EngineWebHostServiceExtensions
    {
        public static void RunAsService(this IWebHost host)
        {   
            Log.i("SRV", "Starting as service..");
            EngineProcess process = (EngineProcess)host.Services.GetService(typeof(EngineProcess));
            var webHostService = new EngineWebHostService(host, process);
            ServiceBase.Run(webHostService);
            Log.i("SRV", "Service started.");
        }

        public static void RunAsApp(this IWebHost host)
        {
            Log.i("SRV", "Starting engine...");
            EngineProcess process = (EngineProcess) host.Services.GetService(typeof(EngineProcess));
            process.Start();
            Log.i("SRV", "Engine started..");
            Log.i("SRV", "Starting web host...");
            host.Run();
        }
    }
}