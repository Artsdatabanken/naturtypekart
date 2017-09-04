using System;
using Common.Diagnostic.Network.Web;
using NUnit.Framework;

namespace Test.Smoke
{
    public class PortalApiTest
    {
        private readonly string baseUrl;

        public PortalApiTest()
        {
            var host = "it-webadbtest01.it.ntnu.no";
            host = Environment.GetEnvironmentVariable("TESTHOST") ?? host;
            baseUrl = $"http://{host}/ninproxy_vs2017/Geolocation";
        }

        [Test]
        public void FinnMatrikkelenheter()
        {
            Http.Get($"{baseUrl}/FinnMatrikkelenheter/1601/10/1");
        }

        [Test]
        public void NdToken()
        {
            Http.Get($"{baseUrl}/ndToken");
        }

        [Test]
        public void GeolocationByName()
        {
            Http.Get($"{baseUrl}/geolocationByName/Trondheim");
        }

        [Test]
        public void SøkKommunenummer()
        {
            Http.Get($"{baseUrl}/searchMunicipality/1601");
        }

        [Test]
        public void SøkKommunenavn()
        {
            Http.Get($"{baseUrl}/searchMunicipality/Trondheim");
        }

        [Test]
        public void matrikkelGBNr()
        {
            Http.Get($"{baseUrl}/matrikkelByGBNr/1601_12_14");
        }

        [Test]
        public void HentOmraadeForMatrikkelenhet()
        {
            Http.Get($"{baseUrl}/matrikkelByGBNr/1601_12_14");
        }

        [Test]
        public void GeolocationByGBNr()
        {
            Http.Get($"{baseUrl}/geolocationByGBNr/1601_12_14");
        }
    }
}