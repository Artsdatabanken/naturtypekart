using System.Collections.Specialized;
using System.Net;
using Nin.Configuration;

namespace Nin.Diagnostic
{
    /// <summary>
    /// Send log message via Pushover
    /// https://pushover.net
    /// </summary>
    public static class Pushover
    {
        public static void SendNotification(string message)
        {
            var config = Config.Settings.Diagnostic.Pushover;
            var parameters = new NameValueCollection
            {
                {"token", config.Token},
                {"user", config.User},
                {"message", message}
            };

            using (var client = new WebClient())
                client.UploadValues("https://api.pushover.net/1/messages.json", parameters);
        }
    }
}