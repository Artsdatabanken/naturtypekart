using System.Collections.Generic;
using Nin.Områder;

namespace Nin.Map.Tiles.geojson
{
    public class RootObject
    {
        public string type = "FeatureCollection";
        public Crs crs = new Crs();
        public List<Feature> features { get; set; }
    }

    public class FeatureProperties
    {
        public string Category;
        public string Name;
        public int Number;
        public string Value;
        public AreaType Type;
        public string kind;
        public string nin { get; set; }
    }

    public class Crs
    {
        public string type { get; set; }
        public CrsProperties properties { get; set; }
    }

    public class CrsProperties
    {
        public string name { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<List<List<List<double>>>> coordinates { get; set; }
    }

    public class Feature
    {
        public int id;
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public FeatureProperties properties { get; set; }
    }
}