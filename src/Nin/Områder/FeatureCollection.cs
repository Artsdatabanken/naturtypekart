using System.Collections.ObjectModel;
using System.Globalization;

namespace Nin.Områder
{
    public class FeatureCollection
    {
        public Collection<Feature> Features { get; set; }
        public Crs crs;

        public int GetEpsgCode()
        {
            if (crs == null) return 0;
            string name = crs.properties.name;
            if (!name.StartsWith("EPSG:")) return 0;

            return int.Parse(name.Replace("EPSG:", ""), CultureInfo.InvariantCulture);
        }
    }

    public class Crs
    {    
        public string type ="name";
        public CrsProperties properties = new CrsProperties();
    }

    public class CrsProperties
    {
        public string name; // ie "urn:ogc:def:crs:OGC:1.3:CRS84", "EPSG:32633"
    }
}
