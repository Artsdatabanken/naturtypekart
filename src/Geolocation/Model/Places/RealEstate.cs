using System;
using System.Collections.Generic;

namespace Geolocation.Model.Places
{
    public class RealEstate : Sted
    {
        public RealEstate(int gnr, int bnr, int fnr, int snr, double area, string useOfProp, string realEstateType, bool registered, int kommunenummer, DateTime sistOppdatert, List<Koordinat> koordinatliste)
        {
            this.Gnr = gnr;
            this.Bnr = bnr;
            this.Fnr = fnr;
            this.Snr = snr;
            this.Area = area;
            this.UseOfProperty = useOfProp;
            this.RealEstateType = realEstateType;
            this.OficiallyRegistered = registered;
            this.Kommunenummer = kommunenummer;
            this.SistOppdatert = sistOppdatert;
            this.Koordinatliste = koordinatliste;
        }
        public int Bnr { get; set; }
        public int Gnr { get; set; }
        public int Fnr { get; set; }
        public int Snr { get; set; }
        public double Area { get; set; }
        public string UseOfProperty { get; set; }
        public string RealEstateType { get; set; }
        public bool OficiallyRegistered { get; set; }
        public int Kommunenummer { get; set; }
        public DateTime SistOppdatert { get; set; }
        public List<Koordinat> Koordinatliste { get; set; }
    }
}