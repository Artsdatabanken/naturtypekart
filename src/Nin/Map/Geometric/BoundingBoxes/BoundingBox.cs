using System;
using System.Globalization;
using DotSpatial.Topology;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Point = Nin.Map.Tiles.Geometri.Point;

namespace Nin.Common.Map.Geometric.BoundingBoxes
{
    [Serializable]
    public class BoundingBox
    {
        public Point LowerLeft;
        public Point UpperRight;

        public BoundingBox()
        {
        }

        public BoundingBox(Point lowerLeft, Point upperRight)
        {
            if (lowerLeft.X > upperRight.X) throw new ArgumentException();
            if (lowerLeft.Y > upperRight.Y) throw new ArgumentException();
            LowerLeft = lowerLeft;
            UpperRight = upperRight;
        }

        public BoundingBox(double left, double top, double right, double bottom) : this(new Point(left, bottom), new Point(right, top))
        {
        }

        [JsonIgnore]
        public double Left => LowerLeft.X;
        [JsonIgnore]
        public double Top => UpperRight.Y;
        [JsonIgnore]
        public double Bottom => LowerLeft.Y;
        [JsonIgnore]
        public double Right => UpperRight.X;

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}, {1}]", LowerLeft, UpperRight);
        }

        public BoundingBox Union(BoundingBox other)
        {
            double left = Math.Min(Left, other.Left);
            double bottom = Math.Min(Bottom, other.Bottom);
            double right = Math.Max(Right, other.Right);
            double top = Math.Max(Top, other.Top);
            if (right - left < 0) return Empty;
            if (top - bottom < 0) return Empty;
            return new BoundingBox(left, top, right, bottom);
        }

        public BoundingBox Intersect(BoundingBox other)
        {
            double left = Math.Max(Left, other.Left);
            double bottom = Math.Max(Bottom, other.Bottom);
            double right = Math.Min(Right, other.Right);
            double top = Math.Min(Top, other.Top);
            if (right - left < 0) return Empty;
            if (top - bottom < 0) return Empty;
            return new BoundingBox(left, top, right, bottom);
        }

        public static readonly BoundingBox Empty = new BoundingBox(0, 0, 0, 0);

        public BoundingBox(IEnvelope e)
        {
            LowerLeft = new Point(e.Left(), e.Bottom());
            UpperRight = new Point(e.Right(), e.Top());
        }

        public bool Intersects(BoundingBox other) => !Intersect(other).IsEmpty;

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                if (Width <= 0) return true;
                return (Height <= 0);
            }
        }

        [JsonIgnore]
        public double Width => Right - Left;
        [JsonIgnore]
        public double Height => Top - Bottom;

        public static BoundingBox From(SqlGeometry geometry)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            for (int i = 1; i <= geometry.STNumPoints(); i++)
            {
                var p = geometry.STPointN(i);
                double x = (double)p.STX;
                double y = (double)p.STY;
                if (x > maxX) maxX = x;
                if (x < minX) minX = x;
                if (y > maxY) maxY = y;
                if (y < minY) minY = y;
            }

            return new BoundingBox(minX, maxY, maxX, minY);
        }
    }
}
