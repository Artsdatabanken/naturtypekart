namespace Geolocation.Model.Places
{
    public class Kommune : Sted
    {
        public Kommune(string fylkenavn, string kommunenavn, int kommunenummer)
        {
            Fylkenavn = fylkenavn;
            Kommunenavn = kommunenavn;
            Kommunenummer = kommunenummer;

            Id = kommunenummer;
            Navn = kommunenavn;
            Beskrivelse = kommunenavn + ", " + fylkenavn;
        }

        public string Fylkenavn { get; set; }
        public string Kommunenavn { get; set; }
        public int Kommunenummer { get; set; }
    }
}