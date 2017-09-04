using System;
using Common.Diagnostic.Network.Web;
using NUnit.Framework;

namespace Test.Smoke
{
    public class ProxyTest
    {
        private readonly string baseUrl;

        public ProxyTest()
        {
            var host = "it-webadbtest01.it.ntnu.no";
            host = Environment.GetEnvironmentVariable("TESTHOST") ?? host;
            baseUrl = $"{host}/NinProxy/";
        }

        [Test]
        public void Xkcd()
        {
            Http.Get($"http://{baseUrl}/?url=http://xkcd.com");
        }
    }
}