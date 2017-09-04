using DotSpatial.Topology;
using Nin.Common.Map.Geometric.BoundingBoxes;

namespace Nin.Common.Map.Tiles
{
    public static class WebMercator
    {
        public static BoundingBox BoundingBox1 => new BoundingBox(-20037508.342789244, 20037508.342789244, 20037508.342789244, -20037508.342789244);
        public static Envelope BoundingBox => new Envelope(-20037508.342789244, 20037508.342789244, 20037508.342789244, -20037508.342789244);
    }

    public static class Utm33
    {
        // EPSG 32633
        //public static BoundingBox BoundingBox1 => new BoundingBox(-20037508.342789244, 20037508.342789244, 20037508.342789244, -20037508.342789244);
        public static Envelope BoundingBox => new Envelope(-2500000, 3500000, 3045984, 9045984);
    }
}