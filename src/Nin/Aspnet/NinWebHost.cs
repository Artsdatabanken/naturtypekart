using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Nin.Common;
using Nin.Configuration;
using Nin.Diagnostic;

namespace Nin.Aspnet
{
    public static class NinWebHost
    {
        public static IWebHost CreateWith<T>() where T : CommonStartup
        {
            Console.WriteLine("Hello, my name is " + Environment.UserName + ".");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Config.Settings = Config.LoadFromExecutablePath();

            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<T>()
                .UseLoggerFactory(new NinLoggerFactory())
                .Build();
            return host;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            Log.e("API", exception);
            throw exception;
        }
    }

    public class NinLoggerFactory : ILoggerFactory
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DotnetCoreLoggerWrapper();
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }
    }

    public class DotnetCoreLoggerWrapper : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (exception == null)
                Nin.Diagnostic.Log.Write("NET", Map(logLevel), state.ToString());
            else
                Nin.Diagnostic.Log.e("NET", exception);
        }

        static LogPriority Map(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return LogPriority.Verbose;
                case LogLevel.Debug:
                    return LogPriority.Debug;
                case LogLevel.Information:
                    return LogPriority.Info;
                case LogLevel.Warning:
                    return LogPriority.Warn;
                case LogLevel.Error:
                    return LogPriority.Error;
                case LogLevel.Critical:
                    return LogPriority.Error;
                case LogLevel.None:
                    return LogPriority.Crazy;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new DisposeNothing();
        }
    }

    public class DisposeNothing : IDisposable
    {
        public void Dispose()
        {
        }
    }
}