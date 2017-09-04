using System;
using Common.Diagnostic.Network.Web;
using NUnit.Framework;

namespace Test.Smoke
{
    public class AsiWebTest
    {
        private readonly string baseUrl;

        [Test]
        public void NewUser()
        {
            Http.Get($"{baseUrl}/UserSystem/AnonymousUsers/NewUser.aspx");
        }

        [Test]
        public void Logginn()
        {
            Http.Get($"{baseUrl}/LogInn.aspx");
        }

        public AsiWebTest()
        {
            var host = "it-webadbtest01.it.ntnu.no";
            host = Environment.GetEnvironmentVariable("TESTHOST") ?? host;
            baseUrl = $"http://{host}/ASIWeb";
        }
    }
}