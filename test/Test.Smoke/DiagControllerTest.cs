using System;
using Common.Diagnostic.Network.Web;
using NUnit.Framework;

namespace Test.Smoke
{
    public class DiagControllerTest
    {
        [Test]
        public void Diag()
        {
            Http.Get($"{baseUrl}");
        }

        private readonly string baseUrl;

        public DiagControllerTest()
        {
            var host = "it-webadbtest01.it.ntnu.no";
            host = Environment.GetEnvironmentVariable("TESTHOST") ?? host;
            baseUrl = $"http://{host}/NbicDocumentStoreApi/diag";
        }

    }
}