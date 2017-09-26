using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Geolocation;
using Geolocation.Model;
using Geolocation.Model.Coordinates;
using Geolocation.Model.Places;
using Geolocation.Utils;
using Microsoft.AspNetCore.Http; // Needed for the SetString and GetString extension methods
using Microsoft.AspNetCore.Mvc;
using Nin.Aspnet;
using Nin.Configuration;
using Nin.Diagnostic;
using Nin.Geolocation.Model.Enums;
using Nin.Geolocation.Utils;
using Types;

namespace Api.Proxy.Controllers
{
    /// <summary>
    /// Geographical location services
    /// </summary>
    public class GeoLocationController : Controller
    {
        public ContentResult Index()
        {
            Log.v("AUTH", new NinHtmlResult("Running ok").Content);
            return new NinHtmlResult("Running ok");
        }


        private readonly ILocationService stedstjeneste = new LocationService();

        /// <summary>
        /// Adresse and location name search
        /// </summary>
        /// <param name="term">Search string (start of adress or location)</param>
        /// <returns>List of adresses and locations with geographical coordinates</returns>
        public IActionResult GeolocationByName(string term)
        {
            var locations = stedstjeneste.FinnLokalitet(term);
            foreach (var locality in locations)
            {
                locality.ProductTypeLevel = NavneTyper.GetLevelForType(locality.Product);

                //stedstjeneste.FinnLokalitet mixes up x and y
                // - cannot change stedstjeneste.FinnLokalitet because mobile app uses the service
                locality.Koordinat.SwapXAndY();

                //locality.Koordinat =
                //    (GoogleMercatorKoordinat)CoordinateTransformer.TransformCoordinate(locality.Koordinat, (int)Koordinatsystem.GoogleMercator);
                locality.Koordinat =
                    (UtmKoordinat)CoordinateTransformer.TransformCoordinate(locality.Koordinat, (int)Koordinatsystem.EurefUtmZone33N);
            }
            return Ok(locations);
        }

        /// <summary>
        /// Kommunesearch
        /// </summary>
        /// <param name="søkebegrep">Search string (start of adress or location)</param>
        /// <returns>List of adresses and locations with geographical coordinates</returns>
        public List<Kommune> FinnKommune(string søkebegrep)
        {
            int knr;
            int.TryParse(søkebegrep, out knr);
            return stedstjeneste.FinnKommune(knr, "*");
        }

        /// <summary>
        /// Gårds- og Bruksnummer search
        /// </summary>
        /// <param name="gbnrString">Gårds og bruksnummer in the format 'kommunenummer_gårdsnummer_bruksnummer</param>
        /// <returns>List of gårds og bruksnummer (0 or 1 items) with geographical coordinates</returns>
        public IActionResult GetRealEstateData(string gbnrString)
        {
            var values = gbnrString.Split('_');
            var kommuneNr = int.Parse(values[0]);
            var gaardsnr = int.Parse(values[1]);
            var bruksNr = int.Parse(values[2]);
            var locations = stedstjeneste.GetRealEstateData(kommuneNr, gaardsnr, bruksNr, 0, 0);
            // (festNr = 0, seksjonsNr = 0);
            var location = locations.FirstOrDefault();
            var coords = new List<Koordinat>();
            foreach (var coord in location.Koordinatliste)
            {
                coords.Add(
                    (GoogleMercatorKoordinat)
                        CoordinateTransformer.TransformCoordinate(coord, (int)Koordinatsystem.GoogleMercator));
            }
            location.Koordinatliste = coords;
            return Ok(locations);
        }

        /// <summary>
        /// Find area of property
        /// </summary>
        public IActionResult HentOmraadeForMatrikkelenhet(int kommuneNr, int gaardsnr, int bruksNr, int festNr, int seksjonsNr)
        {
            var locations = stedstjeneste.HentOmraadeForMatrikkelenhet(kommuneNr, gaardsnr, bruksNr, festNr, seksjonsNr);
            return Ok(locations);
        }

        /// <summary>
        /// Find matrikkelenheter, trenger minst knr og gaardsnr
        /// </summary>
        /// <returns>geographical coordinates</returns>
        public IActionResult FinnMatrikkelenheter(int kommuneNr, int gaardsnr, int bruksNr)
        {
            var locations = stedstjeneste.FinnMatrikkelenheter(kommuneNr, gaardsnr, bruksNr);
            return Ok(locations);
        }

        public IActionResult NdToken()
        {
            var clientIp = FindClientIp();

            Token token = new Token();

            var tokenKey = "ndtoken" + clientIp;
            var tokenString = HttpContext.Session.GetString(tokenKey);
            if (!string.IsNullOrEmpty(tokenString))
            {
                var parts = tokenString.Split(';');
                DateTime expires;
                if (!DateTime.TryParse(parts[1], out expires))
                    expires = DateTime.UtcNow.AddMinutes(-5);

                token = new Token(clientIp, parts[0], expires);
            }

            int ttlMinutes = Config.Settings.ExternalDependency.GeoNorge.TokenMinutesValid;
            if (token.value == "" || token.expires < DateTime.UtcNow.AddMinutes(10))
            {
                var requestNdToken = stedstjeneste.RequestNDToken(clientIp, ttlMinutes);
                token = new Token(clientIp, requestNdToken, ttlMinutes);
            }

            tokenString = token.value + ';' + token.expires.ToString("yyyy-MM-ddThh:mm:ss") + ';' + clientIp;

            if (token.value != "")
                HttpContext.Session.SetString(tokenKey, tokenString);

            return Ok(tokenString);
        }

        private string FindClientIp()
        {
            IPAddress address = Request.HttpContext.Connection.LocalIpAddress;
            string clientIp = "";
            if (address != null)
                clientIp = address.ToString();

            if (clientIp == "" || clientIp == "::1" || clientIp == "127.0.0.1")
                return Config.Settings.ExternalDependency.GeoNorge.ProxyServerIP;
            return clientIp;
        }
    }
}
