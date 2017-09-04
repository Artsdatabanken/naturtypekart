using Nin.Common.Map.Geometric.BoundingBoxes;
using NUnit.Framework;

namespace Nin.Test.Unit.Tile.Clipping
{
    public class BoundingBoxTests
    {
        [Test]
        public void Intersect()
        {
            BoundingBox b1 = new BoundingBox(5, 10, 11, 4);
            BoundingBox b2 = new BoundingBox(6, 12, 9, 3);
            BoundingBox actual = b1.Intersect(b2);
            Assert.AreEqual("[[6,4], [9,10]]", actual.ToString());
        }
    }
}