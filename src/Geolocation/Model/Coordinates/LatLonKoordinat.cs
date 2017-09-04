namespace Geolocation.Model.Coordinates
{
    public class LatLonKoordinat : Koordinat
    {
        public LatLonKoordinat()
        {
            Koordinatsystem = (int)Nin.Geolocation.Model.Enums.Koordinatsystem.Wgs84LatLon;
        }
    }
}