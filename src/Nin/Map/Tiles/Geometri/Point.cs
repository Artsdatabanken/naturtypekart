using System;
using System.Globalization;

namespace Nin.Map.Tiles.Geometri
{
    [Serializable]
    public class Point
    {
        public readonly double X;
        public readonly double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0},{1}]", Math.Round(X, 1), Math.Round(Y, 1));
        }

        public static Vector operator -(Point a, Point b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static Vector operator +(Point a, Point b)
        {
            return new Vector(b.X + a.X, b.Y + a.Y);
        }

        public static Vector operator +(Point a, Vector b)
        {
            return new Vector(b.X + a.X, b.Y + a.Y);
        }

        public static Point operator *(Point a, double factor)
        {
            return new Point(a.X * factor, a.Y * factor);
        }
    }
}