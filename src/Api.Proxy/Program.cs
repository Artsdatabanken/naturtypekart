using Microsoft.AspNetCore.Hosting;
using Nin.Aspnet;
using Nin.Configuration;
using Nin.Diagnostic;

namespace Api.Proxy
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = NinWebHost.CreateWith<Startup>();

            Log.i("PROX", "Allow list:\r\n" + string.Join("\r\n", Config.Settings.Proxy.AllowedUrlPrefixes));

            // Necessary for LocationService to work
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();

            host.Run();
        }
    }
}
