using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Nin.Configuration;
using Nin.Diagnostic;

namespace Engine
{
    /// <summary>
    /// Install service:
    /// sc.exe create Naturtyper binPath="C:\Users\B\Source\Naturtypekart\src\Nin.Engine\bin\Debug\net46\win7-x64\Nin.Engine.exe"
    /// </summary>
    static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Config.Settings = Config.LoadFrom(path);

                GoGoGo(args);
            }
            catch (Exception caught)
            {
                Console.WriteLine(caught.ToString());
                Environment.ExitCode = 99;
            }
            return Environment.ExitCode;
        }

        private static void GoGoGo(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            var exePath = Process.GetCurrentProcess().MainModule.FileName;
            var directoryPath = Path.GetDirectoryName(exePath);

            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(directoryPath)
                .UseStartup<Startup>()
                .UseUrls("http://+:4444")
                .Build();
            if (Debugger.IsAttached || args.Contains("--debug") || Environment.UserInteractive)
                host.RunAsApp();
            else
                host.RunAsService();
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.e("SRV", (Exception)e.ExceptionObject);
        }
    }
}