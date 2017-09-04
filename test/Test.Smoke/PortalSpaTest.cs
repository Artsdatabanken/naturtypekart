using System;
using Common.Diagnostic.Network.Web;
using NUnit.Framework;

namespace Test.Smoke
{
    public class PortalSpaTest
    {
        private readonly string baseUrl;

        public PortalSpaTest()
        {
            var host = "it-webadbtest01.it.ntnu.no";
            host = Environment.GetEnvironmentVariable("TESTHOST") ?? host;
            baseUrl = $"http://{host}/NBicPortalSpa";
        }

        [Test]
        public void Index_html()
        {
            Http.Get($"{baseUrl}/index.html");
        }
    }
}