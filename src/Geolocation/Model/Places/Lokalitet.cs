using System;

namespace Geolocation.Model.Places
{
    [Serializable]
    public class Lokalitet : Sted
    {
        public Lokalitet(string kommunenavn, int kommunenummer, string stedsnavn, int nameType, string språk, string product, string typeDescription, Koordinat koordinat)
        {
            Kommunenavn = kommunenavn;
            Kommunenummer = kommunenummer;
            Stedsnavn = stedsnavn;
            NameType = nameType;
            Språk = språk;
            Product = product;
            TypeDescription = typeDescription;
            Navn = stedsnavn;
            Koordinat = koordinat;
            Beskrivelse = product;
        }

        public Lokalitet(string stedsnavn, string product, Koordinat koordinat)
        {
            Navn = stedsnavn;
            Koordinat = koordinat;
            Beskrivelse = product;
        }

        public string Kommunenavn { get; set; }
        public int Kommunenummer { get; set; }
        public string CountyName { get; set; }
        public int CountyId { get; set; }
        public string Stedsnavn { get; set; }
        public int NameType { get; set; }
        public string Språk { get; set; }
        public string Product { get; set; }
        public int ProductTypeLevel { get; set; }
        public string TypeDescription { get; set; }
    }
}