using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using System.Text.RegularExpressions;
using BasisFelles;
using Geolocation.Model.Coordinates;
using Geolocation.Model.Places;
using Nin;
using Nin.Configuration;
using Nin.Geolocation.Model.Enums;
using rep.geointegrasjon.no.Felles.Geometri.xml.schema._2012._01._31;
using Koordinat = Geolocation.Model.Koordinat;
using KoordinatsystemKode = rep.geointegrasjon.no.Felles.Geometri.xml.schema._2012._01._31.KoordinatsystemKode;
using MatrikkelKontekst = rep.geointegrasjon.no.Felles.Teknisk.xml.schema._2012._01._31.MatrikkelKontekst;
using Matrikkelnummer = rep.geointegrasjon.no.Matrikkel.Felles.xml.schema._2012._01._31.Matrikkelnummer;

namespace Geolocation
{
    public class LocationService : ILocationService
    {
        private const string AdresseUrl = "https://ws.geonorge.no/SKWS2/services/Adresse";
        private const string EiendomUrl = "https://ws.geonorge.no/SKWS2/services/Eiendom";
        private const string SsrIndexSearchUrl = "https://ws.geonorge.no:443/SKWS3Index/ssrIndexSearch";
        private const string SsrUrl = "https://ws.geonorge.no/SKWS2/services/SSR";

        private readonly MatrikkelKartServiceClient matrikkelKartKlient;
        private readonly MatrikkelBasisServiceClient matrikkelBasisClient;

        public LocationService()
        {
            matrikkelKartKlient =
                CreateMatrikkelKartServiceClient(
                    "http://www.nd.matrikkel.no/geointegrasjon/matrikkel/wsapi/v1/KartService", BrukerId, Passord);
            matrikkelBasisClient =
                CreateMatrikkelBasisServiceClient(
                    "http://www.nd.matrikkel.no/geointegrasjon/matrikkel/wsapi/v1/BasisService", BrukerId, Passord);
        }

        private static MatrikkelKartServiceClient CreateMatrikkelKartServiceClient(string url, string username, string password)
        {
            var binding = new BasicHttpBinding
            {
                CloseTimeout = new TimeSpan(0, 0, 1, 0),
                OpenTimeout = new TimeSpan(0, 0, 1, 0),
                ReceiveTimeout = new TimeSpan(0, 0, 10, 0),
                SendTimeout = new TimeSpan(0, 0, 1, 0),
                AllowCookies = false,
                BypassProxyOnLocal = false,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                MaxBufferPoolSize = 524288,
                MaxBufferSize = 524288,
                MaxReceivedMessageSize = 524288,
                TextEncoding = Encoding.UTF8,
                TransferMode = TransferMode.Buffered,
                UseDefaultWebProxy = true,
                MessageEncoding = WSMessageEncoding.Text
            };

            var basicHttpSecurity = new BasicHttpSecurity {Mode = BasicHttpSecurityMode.TransportCredentialOnly};

            var httpTransportSecurity =
                new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.Basic,
                    ProxyCredentialType = HttpProxyCredentialType.None,
                    Realm = "default"
                };
            basicHttpSecurity.Transport = httpTransportSecurity;

            var basicHttpMessageSecurity =
                new BasicHttpMessageSecurity
                {
                    ClientCredentialType = BasicHttpMessageCredentialType.UserName,
                    AlgorithmSuite = SecurityAlgorithmSuite.Default
                };
            basicHttpSecurity.Message = basicHttpMessageSecurity;

            binding.Security = basicHttpSecurity;

            var endpointAddress = new EndpointAddress(
                new Uri(url), AddressHeader.CreateAddressHeader("contract", "",
                    "GImatrikkelWS.MatrikkelkartPort.MatrikkelKartService"));

            return new MatrikkelKartServiceClient(binding, endpointAddress)
            {
                ClientCredentials =
                {
                    UserName =
                    {
                        UserName = username.ToLower(),
                        Password = password
                    }
                }
            };
        }

        private static MatrikkelBasisServiceClient CreateMatrikkelBasisServiceClient(string url, string username,
            string password)
        {
            var binding = new BasicHttpBinding
            {
                CloseTimeout = new TimeSpan(0, 0, 1, 0),
                OpenTimeout = new TimeSpan(0, 0, 1, 0),
                ReceiveTimeout = new TimeSpan(0, 0, 10, 0),
                SendTimeout = new TimeSpan(0, 0, 1, 0),
                AllowCookies = false,
                BypassProxyOnLocal = false,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                MaxBufferPoolSize = 524288,
                MaxBufferSize = 524288,
                MaxReceivedMessageSize = 524288,
                TextEncoding = Encoding.UTF8,
                TransferMode = TransferMode.Buffered,
                UseDefaultWebProxy = true,
                MessageEncoding = WSMessageEncoding.Text
            };

            var basicHttpSecurity = new BasicHttpSecurity {Mode = BasicHttpSecurityMode.TransportCredentialOnly};

            var httpTransportSecurity =
                new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.Basic,
                    ProxyCredentialType = HttpProxyCredentialType.None,
                    Realm = ""
                };
            basicHttpSecurity.Transport = httpTransportSecurity;

            var basicHttpMessageSecurity =
                new BasicHttpMessageSecurity
                {
                    ClientCredentialType = BasicHttpMessageCredentialType.UserName,
                    AlgorithmSuite = SecurityAlgorithmSuite.Default
                };
            basicHttpSecurity.Message = basicHttpMessageSecurity;

            binding.Security = basicHttpSecurity;

            var endpointAddress = new EndpointAddress(
                new Uri(url), AddressHeader.CreateAddressHeader("contract", "", "GImatrikkelWS.BasisService.MatrikkelBasisService"));

            return new MatrikkelBasisServiceClient(binding, endpointAddress)
            {
                ClientCredentials =
                {
                    UserName =
                    {
                        UserName = username.ToLower(),
                        Password = password
                    }
                }
            };
        }

        private static AdresseClient CreateAdresseClient(string url)
        {
            var endpointAddress = new EndpointAddress(
                new Uri(url),
                AddressHeader.CreateAddressHeader("contract", "", "geonorge.Adresse.Adresse")
                );

            return new AdresseClient(CreateBasicHttpBinding(), endpointAddress);
        }

        private static EiendomClient CreateEiendomClient(string url)
        {
            var endpointAddress = new EndpointAddress(
                new Uri(url),
                AddressHeader.CreateAddressHeader("contract", "", "geonorge.Eiendom.Eiendom")
                );

            return new EiendomClient(CreateBasicHttpBinding(), endpointAddress);
        }

        private EiendomByggClient CreateEiendomByggClient(string url)
        {
            var endpointAddress = new EndpointAddress(
                new Uri(url),
                AddressHeader.CreateAddressHeader("contract", "", "geonorge.EiendomBygg.EiendomBygg")
                );

            return new EiendomByggClient(CreateBasicHttpBinding(), endpointAddress);
        }

        private SokKomDataClient CreateSokKomDataClient(string url)
        {
            var endpointAddress = new EndpointAddress(
                new Uri(url),
                AddressHeader.CreateAddressHeader("contract", "", "geonorge.KommuneData.SokKomData")
                );

            return new SokKomDataClient(CreateBasicHttpBinding(), endpointAddress);
        }

        private static ssrIndexSearchClient CreateSsrIndexSearchClient(string url)
        {
            var endpointAddress = new EndpointAddress(
                new Uri(url),
                AddressHeader.CreateAddressHeader("contract", "", "geonorge.Stedsnavn.ssrIndexSearch")
                );

            return new ssrIndexSearchClient(CreateBasicHttpBinding(), endpointAddress);
        }

        private static SSRClient CreateSsrClient(string url)
        {
            var endpointAddress = new EndpointAddress(
                new Uri(url),
                AddressHeader.CreateAddressHeader("contract", "", "geonorge.StedsRegister.SSR")
                );

            return new SSRClient(CreateBasicHttpBinding(), endpointAddress);
        }

        private static BasicHttpBinding CreateBasicHttpBinding()
        {
            var basicHttpBinding = new BasicHttpBinding();
            var basicHttpSecurity = new BasicHttpSecurity {Mode = BasicHttpSecurityMode.Transport};
            var httpTransportSecurity = new HttpTransportSecurity();
            basicHttpSecurity.Transport = httpTransportSecurity;

            basicHttpBinding.Security = basicHttpSecurity;
            return basicHttpBinding;
        }

        private static string NdToken(string clientIp, int ttl)
        {
            var request =
                WebRequest.Create("http://gatekeeper1.geonorge.no/BaatGatekeeper/gktoken?ip=" +
                                  clientIp + "&min=" + ttl);

            request.Timeout = 10000;
            request.Method = WebRequestMethods.Http.Get;

            var response = request.GetResponse();
            using (Stream s = response.GetResponseStream())
            using (var responseStream = new StreamReader(s))
                return responseStream.ReadToEnd();
        }

        private static string FailoverNdToken(string clientIp, int ttl)
        {
            string token;

            var request = WebRequest.Create("http://pavlov.itea.ntnu.no/BaatService/BAATServices.asmx");

            request.Timeout = 10000;
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/soap+xml; charset=utf-8"; //Soap, Schmoap

            // NB: hardcoded "Jomba"
            string body = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                          +
                          "<soap12:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\">"
                          + "<soap12:Body>"
                          + "<RequestToken xmlns=\"http://www.artsdatabanken.no/test/\">"
                          + "<clientIP>" + clientIp + "</clientIP>"
                          + "<TokenMinutesValid>" + ttl + "</TokenMinutesValid>"
                          + "<Jomba>Piril</Jomba>"
                          + "</RequestToken>"
                          + "</soap12:Body>"
                          + "</soap12:Envelope>";

            UTF8Encoding enc = new UTF8Encoding();
            byte[] data = enc.GetBytes(body);

            request.ContentLength = data.Length;

            using (Stream reqStream = request.GetRequestStream())
                reqStream.Write(data, 0, data.Length);

            var response = request.GetResponse();
            using (Stream s = response.GetResponseStream())
            using (var responseStream = new StreamReader(s))
                token = responseStream.ReadToEnd();

            token = Regex.Replace(token, @"^.+ing>([A-Z0-9]+)<.+$", "$1");

            return token;
        }

        private static string AliasId => Config.Settings.ExternalDependency.GeoNorge.AliasId;
        private static string BrukerId => Config.Settings.ExternalDependency.GeoNorge.BrukerId;
        private static string Passord => Config.Settings.ExternalDependency.GeoNorge.Passord;

        public List<Model.Places.Adresse> AdressSearch(
            string gatenavn, string husbokstav, int gatenummer, int postnummer, string kommuneNavn)
        {
            var result = new List<Model.Places.Adresse>();

            var client = CreateAdresseClient(AdresseUrl);
            var resultat = client.adresseSok2(
                BrukerId,
                Passord,
                AliasId,
                postnummer,
                postnummer,
                kommuneNavn,
                0,
                0,
                gatenavn,
                string.Empty,
                gatenummer,
                gatenummer,
                husbokstav,
                husbokstav,
                0,
                0,
                10,
                23);

            if (resultat.sokStatus.ok)
            {
                result.AddRange(
                    from adr in resultat.adresseRecords2
                    let coord =
                        new UtmKoordinat((int)Koordinatsystem.EurefUtmZone33N)
                        {
                            X = adr.xKoordinat,
                            Y = adr.yKoordinat
                        }
                    select
                        new Model.Places.Adresse(
                            adr.kommunenavn.Trim(),
                            adr.kommunenr,
                            adr.postKretsNavn.Trim(),
                            adr.postKretsNr,
                            adr.gateNavn.Trim(),
                            adr.gateNr,
                            adr.husNr,
                            adr.husUnderNr,
                            adr.husBokstav.Trim(),
                            coord));
            }

            return result;
        }

        public List<County> GetCounties()
        {
            var client = CreateAdresseClient(AdresseUrl);
            var tmpRes = client.fylkeSok(BrukerId, Passord, AliasId);

            return (from fylke in tmpRes.fylker
                    let kommuneliste =
                        fylke.kom.Select(rec => new Kommune(fylke.navn.Trim(), rec.navn.Trim(), rec.nr)).ToList()
                    select new County(fylke.navn.Trim(), fylke.nr, kommuneliste)).ToList();
        }

        public List<Lokalitet> GetLocation(string navn)
        {
            var result = new List<Lokalitet>();
            var client = CreateSsrClient(SsrUrl);

            var tmpRes = client.ssrSok(BrukerId, Passord, AliasId, navn, string.Empty, 0, 0, 0, 0, 0, 23);

            if (tmpRes.sokStatus.ok)
            {
                result.AddRange(
                    from loc in tmpRes.ssrRecords
                    let coord =
                        new UtmKoordinat((int)Koordinatsystem.EurefUtmZone33N)
                        {
                            X = loc.for_nord1,
                            Y = loc.for_aust1
                        }
                    select
                        new Lokalitet(
                            loc.kom_navn.Trim(),
                            loc.for_komm,
                            loc.for_snavn.Trim(),
                            loc.enh_navntype,
                            loc.enh_snspraak.Trim(),
                            loc.for_produkt.Trim(),
                            loc.nty_tekst.Trim(),
                            coord));
            }

            return result;
        }

        public List<Kommune> HentKommunerIFylke(int fylkesnr)
        {
            var client = CreateAdresseClient(AdresseUrl);

            var countyList = client.fylkeSok(BrukerId, Passord, AliasId).fylker.ToList();

            if (fylkesnr != 0)
                countyList = countyList.Where(f => f.nr == fylkesnr).ToList();

            return (from county in countyList
                    from fylkeKomRec in county.kom
                    select new Kommune(county.navn.Trim(), fylkeKomRec.navn.Trim(), fylkeKomRec.nr)).ToList();
        }

        public List<RealEstate> GetRealEstateData(int kommuneNr, int gaardsnr, int bruksNr, int festNr, int seksjonsNr)
        {
            var client = CreateEiendomClient(EiendomUrl);

            var res = client.edmSok(
                BrukerId, Passord, AliasId, kommuneNr, gaardsnr, bruksNr, festNr, seksjonsNr, 0, 23);

            return (from rs in res.edmRecords
                    let coordinateList =
                        rs.koords.Select(
                            rsCoord =>
                                new UtmKoordinat((int)Koordinatsystem.EurefUtmZone33N)
                                {
                                    Y = rsCoord.nord,
                                    X = rsCoord.aust
                                })
                            .Cast<Koordinat>()
                            .ToList()
                    select
                        new RealEstate(
                            rs.gaardsNummer,
                            rs.bruksNummer,
                            rs.festeNummer,
                            rs.seksjonsNummer,
                            rs.areal,
                            rs.brukAvGrunn.Trim(),
                            rs.eiendomsType.Trim(),
                            rs.tinglyst,
                            rs.kommuneNummer,
                            DateTime.Parse(rs.oppdaterDato),
                            coordinateList)).ToList();
        }

        public List<Lokalitet> FinnLokalitet(string name)
        {
            var result = new List<Lokalitet>();
            var client = CreateSsrIndexSearchClient(SsrIndexSearchUrl);

            if (!name.EndsWith("*"))
                name = name + "*";

            // var sokReq = new sokReq {antPerSide = 10, navn = name, eksakteForst = true, brukerid = BrukerId, passord = Passord, aliasId = AliasId};
            var sokReq = new sokReq
            {
                navn = name,
                eksakteForst = true,
                antPerSide = 25,
                antPerSideSpecified = true,
                epsgKode = 4326,
                epsgKodeSpecified = true
            };

            try
            {
                var res = client.sok(sokReq);

                if (res.sokStatus.ok && res.stedsnavn != null)
                    result.AddRange(from sted in res.stedsnavn
                                    let coord = new LatLonKoordinat { Y = sted.aust, X = sted.nord }
                                    let fylkesnavn = sted.fylkesnavn ?? string.Empty
                                    let kommunenavn = sted.kommunenavn ?? string.Empty
                                    let viktigsted = sted.viktigsted ?? string.Empty
                                    select new Lokalitet(sted.stedsnavn.Trim(), sted.navnetype.Trim(), coord)
                                    {
                                        CountyName = fylkesnavn.Trim(),
                                        Kommunenavn = kommunenavn.Trim(),
                                        Språk = sted.spraak.Trim(),
                                        TypeDescription = viktigsted.Trim(),
                                        Product = sted.navnetype.Trim()
                                    });
            }
            catch (FaultException fx)
            {
                var ex = (Exception)fx;
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                throw ex;
            }

            return result;
        }

        private static string BuildPolygonWkt(Omraade omraade)
        {
            var flate = new StringBuilder();
            var culture = new CultureInfo("en-US"); // to get . instead of , in double->string

            flate.Append("((");
            var ytreAvgrensning = omraade.flate.ytreAvgrensning.lukketKurve;
            foreach (var k in ytreAvgrensning)
                flate.Append(k.x.ToString(culture) + " " + k.y.ToString(culture) + ",");
            flate.Remove(flate.Length - 1, 1); // remove last ","
            flate.Append(")");

            var indreAvgrensninger = omraade.flate.indreAvgrensning;
            if (indreAvgrensninger.Count > 0)
            {
                flate.Append(",");
                foreach (Ring hull in indreAvgrensninger)
                {
                    flate.Append("(");
                    foreach (var hk in hull.lukketKurve)
                        flate.Append(hk.x.ToString(culture) + " " + hk.y.ToString(culture) + ",");
                    flate.Remove(flate.Length - 1, 1); // remove last ","
                    flate.Append("),");
                }
                flate.Remove(flate.Length - 1, 1); // remove last ","
            }
            flate.Append(")");

            return flate.ToString();
        }

        public string HentOmraadeForMatrikkelenhet(int kommuneNr, int gaardsnr, int bruksNr, int festNr, int seksjonsNr)
        {
            var wktFlate = "POLYGON EMPTY";
            if (kommuneNr == 0 || gaardsnr == 0 || bruksNr == 0)
                return wktFlate;
            var matrikkelNummer = new Matrikkelnummer
            {
                gaardsnummer = gaardsnr,
                bruksnummer = bruksNr,
                kommunenummer = kommuneNr.ToString(),
                festenummer = festNr,
                seksjonsnummer = seksjonsNr
            };
            var matrikkelKontekst = new MatrikkelKontekst
            {
                klientnavn = "NiN-portal",
                klientversjon = "1.0",
                spraak = "",
                systemversjon = "1.1",
                koordinatsystem = new KoordinatsystemKode
                {
                    erGyldig = true,
                    kodeverdi = "22",
                    kodebeskrivelse = "test"
                }
            };
            OmraadeListe omraadeListe = matrikkelKartKlient.HentOmraadeForMatrikkelenhet(matrikkelNummer, matrikkelKontekst);

            var csc = new MapProjection(Config.Settings.Map.SpatialReferenceSystemIdentifier);
            var flate = new StringBuilder();
            switch (omraadeListe.Count)
            {
                case 0:
                    flate.Append("POLYGON EMPTY");
                    break;
                case 1:
                    flate.Append("POLYGON ");
                    flate.Append(BuildPolygonWkt(omraadeListe[0]));
                    break;
                default:
                    if (omraadeListe.Count > 1) // ex: 1103-56/1956
                    {
                        flate.Append("MULTIPOLYGON  (");
                        foreach (Omraade omraade in omraadeListe)
                        {
                            flate.Append(BuildPolygonWkt(omraade));
                            flate.Append(",");
                        }
                        flate.Remove(flate.Length - 1, 1); // remove last ","
                        flate.Append(")");
                    }
                    break;
            }
            // Convert to display-koordinat system
            wktFlate = csc.ReprojectFromWkt(flate.ToString(), 32632);
            return wktFlate;
        }

        public MatrikkelnummerListe FinnMatrikkelenheter(int kommuneNr, int gaardsnr, int bruksNr)
        {
            if (kommuneNr == 0 || gaardsnr == 0)
                return null;
            var matrikkelNummer = new BasisFelles.Matrikkelnummer
            {
                kommunenummer = kommuneNr.ToString(),
                gaardsnummer = gaardsnr,
                bruksnummer = 0,
                festenummer = 0,
                seksjonsnummer = 0
            };
            var matrikkelKontekst = new BasisFelles.MatrikkelKontekst
            {
                klientnavn = "NiN-portal",
                klientversjon = "1.0",
                spraak = "",
                systemversjon = "1.1",
                koordinatsystem = new BasisFelles.KoordinatsystemKode
                {
                    erGyldig = true,
                    kodeverdi = "22",
                    kodebeskrivelse = "test"
                }
            };
            var matrikkelNummerListe = matrikkelBasisClient.FinnMatrikkelenheter(matrikkelNummer, matrikkelKontekst);
            if (bruksNr == 0) return matrikkelNummerListe;

            var filterListe = new MatrikkelnummerListe();
            filterListe.AddRange(matrikkelNummerListe.Where(
                mnr => mnr.bruksnummer.ToString().StartsWith(bruksNr.ToString()) && mnr.festenummer == 0 &&
                       mnr.seksjonsnummer == 0));
            return filterListe;
        }

        public List<Kommune> FinnKommune(int kommuneNummer, string kommuneNavn)
        {
            if (kommuneNummer <= 0 && kommuneNavn.Length <= 1) return null;

            var client = CreateAdresseClient(AdresseUrl);

            var tmpRes = client.kommuneSok(
                BrukerId, Passord, AliasId, kommuneNummer, kommuneNummer, kommuneNavn, 0);

            return
                tmpRes.kommuneRecords.Select(
                    kommune =>
                        new Kommune(kommune.fylkesnavn.Trim(), kommune.kommunenavn.Trim(), kommune.kommunenr))
                    .ToList();
        }

        /// <summary>
        /// Norge Digital ticket request
        /// </summary>
        /// <param name="clientIp">Client IP</param>
        /// <param name="ttlMinutes">Time to live in minutes</param>
        /// <returns>token value string</returns>
        public string RequestNDToken(string clientIp, int ttlMinutes)
        {
            var token = NdToken(clientIp, ttlMinutes);

            if (token == "" || token.Contains("** IP adresse "))
                token = FailoverNdToken(clientIp, ttlMinutes);

            return token;
        }
    }
}