using System;
using Nin.Common.Map.Geometric.BoundingBoxes;

namespace Nin.Map.Tiles
{
    public abstract class QuadTile
    {
        private readonly QuadTile[] tiles = new QuadTile[4];
        readonly double left;
        readonly double right;
        readonly double top;
        readonly double bottom;
        public readonly BoundingBox BoundingBox;
        public TileCoordinates TileCoordinates;

        public QuadTile GetSub(int quadrant)
        {
            if (tiles[quadrant] != null) return tiles[quadrant];

            QuadTile sub = CreateSub(quadrant);
            tiles[quadrant] = sub;
            return sub;
        }

        private QuadTile CreateSub(int quadrant)
        {
            double midx = (right + left) / 2;
            double midy = (top - bottom) / 2;
            switch (quadrant)
            {
                case 0:
                    return Create(left, top, midx, midy, 0,0);
                case 1:
                    return Create(midx, top, right, midy, 1,0);
                case 2:
                    return Create(midx, midy, right, bottom, 1,1);
                case 3:
                    return Create(left, midy, midx, bottom, 0,1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(quadrant));
            }
        }

        protected abstract QuadTile Create(double left, double top, double right, double bottom, int x, int y);

        protected QuadTile(BoundingBox boundingBox, int zoomLevel, int x, int y)
        {
            BoundingBox = boundingBox;
            left = boundingBox.Left;
            right = boundingBox.Right;
            top = boundingBox.Top;
            bottom = boundingBox.Bottom;
            TileCoordinates = new TileCoordinates(zoomLevel, x, y);
        }
    }
}