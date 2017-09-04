using Common;
using DotSpatial.Topology;
using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Common.Map.Tiles;
using Nin.Map.Layers;
using Nin.Map.Tiles;
using NUnit.Framework;
using Test.Unit.Tile.Clipping;

namespace Test.Unit.Tile.Tiles
{
    public class TileRangeTest
    {
        [Test]
        public void BoundsZoom0()
        {
            Assert.AreEqual("0: [0,-1]", GetTileRange("LINESTRING(-20037508.342789244  -20037508.342789244, 20037508.342789244 20037508.342789244)", 0));
        }

        [Test]
        public void BoundsZoom1()
        {
            Assert.AreEqual("1: [[0,-2],[1,-1]]", GetTileRange("LINESTRING(-20037508.342789244 -20037508.342789244, 20037508.342789244 20037508.342789244)", 1));
        }

        [Test]
        public void TileZoom5()
        {
            Assert.AreEqual("5: [[13,-11],[21,-6]]", GetTileRange("LINESTRING(-2734611.123930466 6472076.058962443, 6579699.394787973 12944152.117924888)", 5));
        }

        private string GetTileRange(string geom, int zoomLevel)
        {
            var rectangle = DotSpatialGeometry.FromWkb(geom, 32633);
            TileRange r = tiler.GetTileRange(zoomLevel, (Envelope)rectangle.Envelope);
            return r.ToString();
            //return JsonConvert.SerializeObject(tids).Replace("\"", "'");
        }

        public TileRangeTest()
        {
            TiledVectorLayer layer = new TiledVectorLayer("", bbox, 39135.75848201024 * 4);
            tiler = new VectorTiler(new TestTileStore(layer), layer);
        }

        readonly BoundingBox bbox = WebMercator.BoundingBox1;
        private readonly VectorTiler tiler;
    }
}