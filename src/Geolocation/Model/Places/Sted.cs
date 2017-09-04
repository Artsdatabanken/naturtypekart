using System;

namespace Geolocation.Model.Places
{
    [Serializable]
    public abstract class Sted
    {
        public int Id { get; set; }
        public string Navn { get; set; }
        public Koordinat Koordinat { get; set; }
        public string Beskrivelse { get; set; }
    }
}