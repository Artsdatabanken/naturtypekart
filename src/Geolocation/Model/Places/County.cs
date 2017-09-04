using System.Collections.Generic;

namespace Geolocation.Model.Places
{
    public class County : Sted
    {
        public County(string fylkenavn, int fylkenummer, List<Kommune> kommuner)
        {
            Fylkenavn = fylkenavn;
            Fylkenummer = fylkenummer;
            Kommuner = kommuner;

            Id = fylkenummer;
            Navn = fylkenavn;
            Beskrivelse = fylkenavn;
        }

        public string Fylkenavn { get; set; }
        public int Fylkenummer { get; set; }
        public List<Kommune> Kommuner { get; set; }
    }
}