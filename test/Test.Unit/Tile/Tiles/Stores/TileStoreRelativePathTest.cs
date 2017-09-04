using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Map.Tiles;
using Nin.Map.Tiles.Vectors;
using NUnit.Framework;

namespace Test.Unit.Tile.Tiles.Stores
{
    public class TileStoreRelativePathTest
    {
        [Test]
        public void RootRelativePath()
        {
            Assert.AreEqual("1/2/3", root.TileCoordinates.GetRelativePath());
        }

        [Test]
        public void Child0RelativePath()
        {
            Assert.AreEqual("2/4/6", GetRelativePath(0));
        }

        [Test]
        public void Child1RelativePath()
        {
            Assert.AreEqual("2/5/6", GetRelativePath(1));
        }

        [Test]
        public void Child2RelativePath()
        {
            Assert.AreEqual("2/5/7", GetRelativePath(2));
        }

        [Test]
        public void Child3RelativePath()
        {
            Assert.AreEqual("2/4/7", GetRelativePath(3));
        }

        private string GetRelativePath(int quadrant)
        {
            var quadTile = root.GetSub(quadrant);
            return quadTile.TileCoordinates.GetRelativePath();
        }

        readonly QuadTile root = new VectorQuadTile(new BoundingBox(0, 1, 1, 0), 1, 2, 3);
    }
}