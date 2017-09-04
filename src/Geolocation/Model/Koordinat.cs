namespace Geolocation.Model
{
    public abstract class Koordinat
    {
        public int Koordinatsystem { get; protected set; }

        public double[] Point
        {
            get => new[] { X, Y };

            set
            {
                X = value[0];
                Y = value[1];
            }
        }

        public double X { get; set; }
        public double Y { get; set; }
        public int? MetricCoordinatePrecision { get; set; }

        public void SwapXAndY()
        {
            var n = X;
            X = Y;
            Y = n;
        }
    }
}