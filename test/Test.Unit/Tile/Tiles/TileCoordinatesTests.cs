using Nin.Map.Tiles;
using NUnit.Framework;

namespace Test.Unit.Tile.Tiles
{
    public class TileCoordinatesTests
    {
        [Test]
        public void TileCoordForLowerLeftZoom2()
        {
            var z = TileCoordinates.GetTileCoordinates(-20037508.342789244, -20037508.342789244, 39155, false, new TestTiledVectorLayer());
            Assert.AreEqual("2/0/-4", z.ToString());
        }
    }
}
