using Microsoft.AspNetCore.Hosting;
using Nin.Aspnet;

namespace Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = NinWebHost.CreateWith<Startup>();
            host.Run();
        }
    }

    /// <summary>
    /// Web service startup
    /// </summary>
    public class Startup : CommonStartup
    {
        public Startup(IHostingEnvironment env) : base(env)
        {
        }
    }
}
