namespace Nin.Map.Tiles.Geometri
{
    public class Vector
    {
        public readonly double X;
        public readonly double Y;

        public Vector(double x, double y)
        {
            Y = y;
            X = x;
        }

        public static explicit operator Point(Vector v)
        {
            return new Point(v.X, v.Y);
        }

        public static Vector operator *(double factor, Vector a)
        {
            return new Vector(a.X * factor, a.Y * factor);
        }

        public static Vector operator *(Vector a, double factor)
        {
            return new Vector(a.X * factor, a.Y * factor);
        }
    }
}