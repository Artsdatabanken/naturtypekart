using System;
using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Map.Tiles.Geometri;

namespace Nin.Map.Tiles
{
    public class TileCoordinatesWithBoundingBox  
    {
        public readonly BoundingBox BoundingBox;
        public int ZoomLevel => TileCoordinates.ZoomLevel;
        public int X => TileCoordinates.X;
        public int Y => TileCoordinates.Y;
        public double Left => BoundingBox.Left;
        public double Right => BoundingBox.Right;
        public double Top => BoundingBox.Top;
        public double Bottom => BoundingBox.Bottom;
        public TileCoordinates TileCoordinates;

        public TileCoordinatesWithBoundingBox(BoundingBox boundingBox, int zoomLevel, int x, int y) 
        {
            BoundingBox = boundingBox;
            TileCoordinates = new TileCoordinates(zoomLevel, x, y);
        }

        public double Width => BoundingBox.Right - BoundingBox.Left;
        public double Height => BoundingBox.Top - BoundingBox.Bottom;

        public TileCoordinatesWithBoundingBox CreateSub(int quadrant)
        {
            double left = BoundingBox.Left;
            double top = BoundingBox.Top;
            double right = BoundingBox.Right;
            double bottom = BoundingBox.Bottom;
            double midx = (right + left) / 2;
            double midy = (top + bottom) / 2;
            switch (quadrant)
            {
                case 0:
                    return Create(left, top, midx, midy, X * 2, Y * 2 + 1);
                case 1:
                    return Create(midx, top, right, midy, X * 2 + 1, Y * 2 + 1);
                case 2:
                    return Create(midx, midy, right, bottom, X * 2 + 1, Y * 2);
                case 3:
                    return Create(left, midy, midx, bottom, X * 2, Y * 2);
                default:
                    throw new ArgumentOutOfRangeException(nameof(quadrant));
            }
        }

        private TileCoordinatesWithBoundingBox Create(double left, double top, double right, double bottom, int x, int y)
        {
            var boundingBox = new BoundingBox(left, top, right, bottom);
            return new TileCoordinatesWithBoundingBox(boundingBox, ZoomLevel + 1, x, y);
        }

        public int GetChildTile(Point point)
        {
            var quadrant = GetChildTileQuadrant(BoundingBox, point);
            return quadrant;
        }

        private static int GetChildTileQuadrant(BoundingBox boundingBox, Point point)
        {
            var midX = (boundingBox.Right + boundingBox.Left) / 2;
            var midY = (boundingBox.Top + boundingBox.Bottom) / 2;
            if (point.X < midX)
                return point.Y < midY ? 3 : 0;
            return point.Y < midY ? 2 : 1;
        }

        public override string ToString()
        {
            return $"{ZoomLevel}: ({X},{Y})";
        }
    }
}