using Common;
using NUnit.Framework;

namespace Test.Unit.Tile.Clipping
{
    public class PolygonTests
    {
        [Test]
        public void AxisAlignedBoundingBox()
        {
            var p = DotSpatialGeometry.FromWkb("LINESTRING(-1 1, -5 2, -3 4)", 32633);
            var bb = p.Envelope;
            Assert.AreEqual("Env[-5 : -1, 1 : 4", bb.ToString());
        }
    }
}