using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nin.Aspnet;

namespace Engine
{
    internal class Startup : CommonStartup
    {
        protected override void ConfigureNinServices(IServiceCollection services)
        {
            services.AddSingleton(new EngineProcess());
        }

        public Startup(IHostingEnvironment env) : base(env)
        {
        }
    }
}