using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nin.Aspnet;

namespace Api.Proxy
{
    /// <summary>
    /// Web service startup
    /// </summary>
    public class Startup : CommonStartup
    {
        public Startup(IHostingEnvironment env) : base(env)
        {
        }

        protected override void ConfigureNin(IApplicationBuilder app)
        {
            app.UseSession();
        }

        protected override void ConfigureNinServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.CookieName = ".Nin";
            });
        }
    }
}