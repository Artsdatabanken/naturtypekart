using System.Collections.Generic;
using Geolocation.Model.Places;
using MatrikkelnummerListe = BasisFelles.MatrikkelnummerListe;

namespace Geolocation
{
    public interface ILocationService
    {
        List<RealEstate> GetRealEstateData(int kommuneNr, int gaardsnr, int bruksNr, int festNr, int seksjonsNr);
        List<Kommune> FinnKommune(int kommuneNummer, string kommuneNavn);
        List<Lokalitet> FinnLokalitet(string name);
        string HentOmraadeForMatrikkelenhet(int kommuneNr, int gaardsnr, int bruksNr, int festNr, int seksjonsNr);
        MatrikkelnummerListe FinnMatrikkelenheter(int kommuneNr, int gaardsnr, int bruksNrStr);
        string RequestNDToken(string clientIp, int ttlMinutes);
    }
}