namespace Geolocation.Model.Coordinates
{
    public class GoogleMercatorKoordinat : Koordinat
    {
        public GoogleMercatorKoordinat()
        {
            Koordinatsystem = (int)Nin.Geolocation.Model.Enums.Koordinatsystem.GoogleMercator;
        }
    }
}