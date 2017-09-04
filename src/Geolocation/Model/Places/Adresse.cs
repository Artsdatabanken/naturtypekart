namespace Geolocation.Model.Places
{
    public class Adresse : Sted
    {
        public Adresse(
            string kommunenavn,
            int kommunenummer,
            string poststed,
            int postnummer,
            string gatenavn,
            int gaetnummer,
            int husnummer,
            int husundernummer,
            string husbokstav,
            Koordinat koordinat)
        {
            Kommunenavn = kommunenavn;
            Kommunenummer = kommunenummer;
            Poststed = poststed;
            Postnummer = postnummer;
            Gatenavn = gatenavn;
            Gaetnummer = gaetnummer;
            Husnummer = husnummer;
            Husundernummer = husundernummer;
            Husbokstav = husbokstav;

            Navn = gatenavn + " " + husnummer + husbokstav;
            Koordinat = koordinat;
        }

        public string Husbokstav { get; set; }
        public int Husnummer { get; set; }
        public int Husundernummer { get; set; }
        public string Kommunenavn { get; set; }
        public int Kommunenummer { get; set; }
        public string Poststed { get; set; }
        public int Postnummer { get; set; }
        public string Gatenavn { get; set; }
        public int Gaetnummer { get; set; }
    }
}