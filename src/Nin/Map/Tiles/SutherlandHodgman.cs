using System;
using System.Collections.Generic;
using System.Linq;
using DotSpatial.Topology;
using Point = Nin.Map.Tiles.Geometri.Point;

namespace Nin.Common.Map.Tiles
{
    public static class SutherlandHodgman
    {
        /// <summary>
        ///     This clips the subject polygon against the clip polygon (gets the intersection of the two polygons)
        /// </summary>
        /// <remarks>
        ///     Based on the psuedocode from:
        ///     http://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman
        /// </remarks>
        /// <param name="subjectPoly">Can be concave or convex</param>
        /// <param name="clipPoly">Must be convex</param>
        /// <returns>The intersection of the two polygons (or null)</returns>
        public static IList<Coordinate> GetIntersectedPolygon(IList<Coordinate> subjectPoly, Point[] clipPoly)
        {
            if (subjectPoly.Count < 3 || clipPoly.Length < 3)
                throw new ArgumentException(
                    $"The polygons passed in must have at least 3 points: subject={subjectPoly.Count}, clip={clipPoly.Length}");

            var outputList = subjectPoly.ToList();

            //	Make sure it's clockwise
            if (!IsClockwise(subjectPoly))
                outputList.Reverse();

            //	Walk around the clip polygon clockwise
            foreach (var clipEdge in IterateEdgesClockwise(clipPoly))
            {
                var inputList = outputList.ToList();
                outputList.Clear();

                if (inputList.Count == 0)
                    //	Sometimes when the polygons don't intersect, this list goes to zero.  Jump out to avoid an index out of range exception
                    break;

                var s = inputList[inputList.Count - 1];
                foreach (var e in inputList)
                {
                    if (IsInside(clipEdge, e))
                    {
                        if (!IsInside(clipEdge, s))
                            outputList.Add(Intersect(s, e, clipEdge.From, clipEdge.To));

                        outputList.Add(e);
                    }
                    else if (IsInside(clipEdge, s))
                        outputList.Add(Intersect(s, e, clipEdge.From, clipEdge.To));

                    s = e;
                }
            }

            return outputList.ToArray();
        }

        /// <summary>
        ///     This iterates through the edges of the polygon, always clockwise
        /// </summary>
        private static IEnumerable<Edge> IterateEdgesClockwise(Point[] polygon)
        {
            if (IsClockwise(polygon))
            {
                for (var cntr = 0; cntr < polygon.Length - 1; cntr++)
                    yield return new Edge(polygon[cntr], polygon[cntr + 1]);

                yield return new Edge(polygon[polygon.Length - 1], polygon[0]);
            }
            else
            {
                for (var cntr = polygon.Length - 1; cntr > 0; cntr--)
                    yield return new Edge(polygon[cntr], polygon[cntr - 1]);

                yield return new Edge(polygon[0], polygon[polygon.Length - 1]);
            }
        }

        /// <summary>
        ///     Returns the intersection of the two lines (line segments are passed in, but they are treated like infinite lines)
        /// </summary>
        /// <remarks>
        ///     Got this here:
        ///     http://stackoverflow.com/questions/14480124/how-do-i-detect-triangle-and-rectangle-intersection
        /// </remarks>
        private static Coordinate Intersect(Coordinate line1From, Coordinate line1To, Coordinate line2From, Coordinate line2To)
        {
            var direction1 = line1To - line1From;
            var direction2 = line2To - line2From;
            var dotPerp = direction1.X * direction2.Y - direction1.Y * direction2.X;

            // If it's 0, it means the lines are parallel so have infinite intersection points
            if (IsNearZero(dotPerp))
                throw new ApplicationException("Line segments does not intersect.");

            var c = line2From - line1From;
            var t = (float)((c.X * direction2.Y - c.Y * direction2.X) / dotPerp);

            return line1From + t * direction1;
        }

        private static bool IsInside(Edge edge, Coordinate test)
        {
            var isLeft = IsLeftOf(edge, test);
            if (isLeft == null)
                //	Colinear points should be considered inside
                return true;

            return !isLeft.Value;
        }

        private static bool IsClockwise(IList<Coordinate> polygon)
        {
            for (var cntr = 2; cntr < polygon.Count; cntr++)
            {
                var isLeft = IsLeftOf(new Edge(polygon[0], polygon[1]), polygon[cntr]);
                if (isLeft != null)
                    //	some of the points may be colinear.  That's ok as long as the overall is a polygon

                    return !isLeft.Value;
            }

            throw new ArgumentException("All the points in the polygon are colinear.");
        }

        private static bool IsClockwise(Point[] polygon)
        {
            for (var cntr = 2; cntr < polygon.Length; cntr++)
            {
                var point = polygon[cntr];
                var isLeft = IsLeftOf(new Edge(polygon[0], polygon[1]), new Coordinate(point.X, point.Y));
                if (isLeft != null)
                    //	some of the points may be colinear.  That's ok as long as the overall is a polygon

                    return !isLeft.Value;
            }

            throw new ArgumentException("All the points in the polygon are colinear.");
        }

        /// <summary>
        ///     Tells if the test point lies on the left side of the edge line
        /// </summary>
        private static bool? IsLeftOf(Edge edge, Coordinate test)
        {
            var tmp1 = edge.To - edge.From;
            var tmp2 = test - edge.To;

            var x = tmp1.X * tmp2.Y - tmp1.Y * tmp2.X; //	dot product of perpendicular?

            if (x < 0)
                return false;
            if (x > 0)
                return true;
            //	Colinear points;
            return null;
        }

        private static bool IsNearZero(double testValue)
        {
            return Math.Abs(testValue) <= .000000001d;
        }

        private class Edge
        {
            public Edge(Coordinate from, Coordinate to)
            {
                From = from;
                To = to;
            }

            public readonly Coordinate From;
            public readonly Coordinate To;

            public Edge(Point from, Point to)
            {
                From = new Coordinate(from.X, from.Y);
                To = new Coordinate(to.X, to.Y);
            }
        }
    }
}