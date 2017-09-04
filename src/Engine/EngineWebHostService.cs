using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace Engine
{
    /// <summary>
    /// Wrapper to get Windows support for service start/stop 
    /// </summary>
    internal class EngineWebHostService : WebHostService
    {
        private readonly EngineProcess process;

        public EngineWebHostService(IWebHost host, EngineProcess process) : base(host)
        {
            this.process = process;
        }

        protected override void OnStarting(string[] args)
        {
            process.Start();
            base.OnStarting(args);
        }

        protected override void OnStopping()
        {
            process.Stop();
            base.OnStopping();
        }
    }
}