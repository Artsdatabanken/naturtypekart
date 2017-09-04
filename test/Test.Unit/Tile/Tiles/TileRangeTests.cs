using Nin.Common.Map.Tiles;
using Nin.Map.Layers;
using Nin.Map.Tiles;
using NUnit.Framework;

namespace Test.Unit.Tile.Tiles
{
    public class TileRangeTests
    {
        [Test]
        public void TileRangeZoom0()
        {
            Assert.AreEqual("0: [0,-1]", GetTileRange(0));
        }
    
        [Test]
        public void TileRangeZoom1()
        {
            Assert.AreEqual("1: [[0,-2],[1,-1]]", GetTileRange(1));
        }

        [Test]
        public void TileRangeZoom2()
        {
            Assert.AreEqual("2: [[0,-4],[3,-1]]", GetTileRange(2));
        }

        [Test]
        public void TileRangeZoom3()
        {
            Assert.AreEqual("3: [[0,-8],[7,-1]]", GetTileRange(3));
        }

        [Test]
        public void TileRangeZoom4()
        {
            Assert.AreEqual("4: [[0,-16],[15,-1]]", GetTileRange(4));
        }

        [Test]
        public void TileRangeZoom5()
        {
            Assert.AreEqual("5: [[0,-32],[31,-1]]", GetTileRange(5));
        }

        private static string GetTileRange(int zoomLevel)
        {
            var z = TileRange.GetTileRangeForExtentAndZ(WebMercator.BoundingBox, zoomLevel, new TestTiledVectorLayer());
            return z.ToString();
        }
    }

    internal class TestTiledVectorLayer : TiledVectorLayer
    {
        public TestTiledVectorLayer() : base("test", WebMercator.BoundingBox1, 39135.75848201024 * 4)
        {
        }
    }
}