using System.Collections.Generic;
using DotSpatial.Topology;
using Nin.Common.Map.Tiles;
using NUnit.Framework;
using Point = Nin.Map.Tiles.Geometri.Point;

namespace Test.Unit.Tile.Geometry
{
    public class SutherlandHodgmanTest
    {
        readonly Point[] clipPoly = { new Point(2, 2), new Point(6, 2), new Point(6, 4), new Point(2, 4) };

        [Test]
        public void Inside()
        {
            var poly = GetIntersectedPolygon(new[] { new Point(2, 2), new Point(6, 4), new Point(6, 2), new Point(2, 2) }, clipPoly);
            Assert.AreEqual("[2,2],[6,4],[6,2],[2,2]", ToString(poly));
        }

        [Test]
        public void Outside()
        {
            var poly = GetIntersectedPolygon(new[] { new Point(0, 0), new Point(10, 0), new Point(10, 10), new Point(0, 10), new Point(0, 0) }, clipPoly);
            Assert.AreEqual("[2,2],[2,4],[6,4],[6,2]", ToString(poly));
        }

        private static string ToString(IEnumerable<Coordinate> poly)
        {
            string r = "";
            foreach (var c in poly)
                r += $",[{c.X:f0},{c.Y:f0}]";
            if (r.Length == 0) return "";
            return r.Substring(1);
        }

        [Test]
        public void TriangleOnTheEdge()
        {
            var poly = GetIntersectedPolygon(new[]
            {
                new Point(2, 2), new Point(6, 2), new Point(2, 4), new Point(2, 2),
            }, clipPoly);
            Assert.AreEqual("[2,2],[2,4],[6,2],[2,2]", ToString(poly));
        }

        [Test]
        public void TriangleOutsideLeft()
        {
            var poly = GetIntersectedPolygon(new[] { new Point(0, 0), new Point(6, 2), new Point(2, 4), new Point(0, 0), }, clipPoly);
            Assert.AreEqual("[2,2],[2,4],[2,4],[6,2],[6,2]", ToString(poly));
        }

        private static IList<Coordinate> GetIntersectedPolygon(Point[] points, Point[] clipPoly)
        {
            List<Coordinate> p = new List<Coordinate>();
            foreach (var point in points)
                p.Add(new Coordinate(point.X, point.Y));
            return SutherlandHodgman.GetIntersectedPolygon(p, clipPoly);
        }


        [Test]
        public void TriangleOutsideRight()
        {
            var poly = GetIntersectedPolygon(new[]
            {
                new Point(7, 3), new Point(3, 7), new Point(3, 1), new Point(7, 3)
            }, clipPoly);
            Assert.AreEqual("[6,3],[5,2],[3,2],[3,4],[6,4],[6,4]", ToString(poly));
        }

        [Test]
        public void TopRightCornerTriangle()
        {
            var poly = GetIntersectedPolygon(new[]
            {
                new Point(5, 3), new Point(7, 3), new Point(7, 5), new Point(5, 5), new Point(5, 3)
            }, clipPoly);
            Assert.AreEqual("[5,3],[5,4],[6,4],[6,3],[5,3]", ToString(poly));
        }
    }
}