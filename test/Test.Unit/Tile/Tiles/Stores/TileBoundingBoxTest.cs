using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Map.Tiles;
using Nin.Map.Tiles.Vectors;
using NUnit.Framework;

namespace Test.Unit.Tile.Tiles.Stores
{
    public class TileBoundingBoxTest
    {
        [Test]
        public void RootBoundingBox()
        {
            Assert.AreEqual("[[0,0], [1,1]]", root.BoundingBox.ToString());
        }

        [Test]
        public void Child0BoundingBox()
        {
            Assert.AreEqual("[[0,0.5], [0.5,1]]", GetBoundingBox(0));
        }

        [Test]
        public void Child1BoundingBox()
        {
            Assert.AreEqual("[[0.5,0.5], [1,1]]", GetBoundingBox(1));
        }

        [Test]
        public void Child2BoundingBox()
        {
            Assert.AreEqual("[[0.5,0], [1,0.5]]", GetBoundingBox(2));
        }

        [Test]
        public void Child3BoundingBox()
        {
            Assert.AreEqual("[[0,0], [0.5,0.5]]", GetBoundingBox(3));
        }

        private string GetBoundingBox(int quadrant)
        {
            QuadTile quadTile = root.GetSub(quadrant);
            return quadTile.BoundingBox.ToString();
        }

        readonly QuadTile root = new VectorQuadTile(new BoundingBox(0, 1, 1, 0), 1, 2, 3);
    }
}