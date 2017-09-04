using Common;
using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Common.Map.Tiles;
using Nin.Map.Layers;
using Nin.Map.Tiles;
using Nin.Map.Tiles.Geometri;
using Nin.Områder;
using NUnit.Framework;
using Test.Unit.Tile.Clipping;

namespace Test.Unit.Tile.Tiles
{
    public class TilePolygonTest
    {
        [Test]
        public void TileZero()
        {
            var actual = Tile("POLYGON ((12 13,12 3e7,3e7 3e7,3e7 13,12 13))");
            //Assert.AreEqual(actual, "[{0/0/-1: POLYGON ((12.0000000000000000 13.0000000000000000, 12.0000000000000000 20037507.5595746000000000, 20037509.0156300000000000 20037508.1567385000000000, 20037508.1215607000000000 13.0000000000000000, 12.0000000000000000 13.0000000000000000))]");
            //Assert.AreEqual("[{0/0/-1: LINESTRING ((12.0000000000000000 13.0000000000000000, 12.0000000000000000 20037508.3427892000000000, 20037508.3427892000000000 20037508.3427892000000000, 20037508.3427892000000000 13.0000000000000000, 12.0000000000000000 13.0000000000000000))]", actual);
            const string expected = "[{0/0/-1: POLYGON ((12.0000000000000000 13.0000000000000000, 12.0000000000000000 20037508.3427892000000000, 20037508.3427892000000000 20037508.3427892000000000, 20037508.3427892000000000 13.0000000000000000, 12.0000000000000000 13.0000000000000000))]";
            Assert.AreEqual(expected, actual);
        }

        private string Tile(string geom)
        {
            Område o = new Område(-1, AreaType.Fylke);
            var rectangle = DotSpatialGeometry.FromWkb(geom, 32633);
            tiler.Update(o, rectangle, 0);
            return store.ToString();
        }

        public TilePolygonTest()
        {
            var layer = new TiledVectorLayer("", bbox, 39135.75848201024 * 4) {TileOverlapRatio = 0};
            store = new TestTileStore(layer);
            tiler = new VectorTiler(store, layer);
        }

        readonly BoundingBox bbox = WebMercator.BoundingBox1;
        private readonly VectorTiler tiler;
        private readonly TestTileStore store;
    }
}