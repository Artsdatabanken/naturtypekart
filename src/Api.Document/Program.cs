using System.Configuration;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nin.Aspnet;

namespace Api.Document
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
        
        protected override void ConfigureNin(IApplicationBuilder app)
        {
            // copy appsetting data into ConfigurationManager.AppSettings for Artsdatabanken.Systemintegrasjon2.dll TODO
            var settings = ConfigurationManager.ConnectionStrings[0];
            var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(settings, false);
            settings.Name = "ArtsdatabankenSIConnectionString";
            settings.ConnectionString = "Data Source=;Initial Catalog=;Persist Security Info=True;User ID=;Password=";
        }
    }
}
