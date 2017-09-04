using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Nin.Common.Diagnostic.TextMode;

namespace Common.Diagnostic.Network.Web
{
    /// <summary>
    /// Wrapper for talking to HTTP services.
    /// </summary>
    public abstract class Http : IDisposable
    {
        protected readonly string Url;
        protected HttpClient Client;
        protected abstract string Method { get; }
        protected abstract Task<string> MakeTheCall();

        protected Http(string url)
        {
            Url = url;
        }

        public string Execute()
        {
            var w = new Stopwatch();
            w.Start();

            Client = new HttpClient();
            using (Client)
            {
                try
                {
                    Task<string> task = MakeTheCall();
                    DisplayResult(Url, Method, task, w);

                    return task.Result;
                }
                catch (WebException caught)
                {
                    var message = GetMessage(Url, w, Method, GetErrorMessage(caught));
                    AnsiConsole.WriteLineRed(message);
                    throw;
                }
                catch (Exception caught)
                {
                    var message = GetMessage(Url, w, Method, caught.Message);
                    AnsiConsole.WriteLineRed(message);
                    throw;
                }
            }
        }

        private static void DisplayResult(string url, string method, Task<string> task, Stopwatch w)
        {
            task.Wait();
            if (task.Result.Length <= 0)
                throw new Exception("Response was empty.");
            if (task.Exception != null)
                throw task.Exception;

            var substring = task.Result.Substring(0, Math.Min(task.Result.Length, 100));
            AnsiConsole.WriteLineGreen(GetMessage(url, w, method, substring));
        }

        private static string GetMessage(string url, Stopwatch w, string method, string response)
        {
            var message = $"{w.ElapsedMilliseconds.ToString("f0"),5:#} ms  {url} ({method}): {response}";
            return message;
        }

        private static string GetErrorMessage(WebException caught)
        {
            var webResponse = caught.Response;
            var responseStream = webResponse.GetResponseStream();
            var msg = caught.Message;
            if (responseStream != null)
                msg = new StreamReader(responseStream).ReadToEnd();
            return "\r\n     " + msg;
        }

        public static string Get(string url)
        {
            Get get = new Get(url);
            return get.Execute();
        }

        public static void Post(string url, string body, string mediaType = "application/json")
        {
            Post post = new Post(url, body, mediaType);
            post.Execute();
        }

        public virtual void Dispose()
        {
            Client.Dispose();
        }
    }
}