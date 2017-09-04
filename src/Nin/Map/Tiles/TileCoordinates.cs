using System;
using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Map.Layers;
using Nin.Map.Tiles.Geometri;

namespace Nin.Map.Tiles
{
    public struct TileCoordinates
    {
        public readonly int ZoomLevel;
        public readonly int X;
        public readonly int Y;

        public TileCoordinates(int zoomLevel, int x, int y)
        {
            Y = y;
            X = x;
            ZoomLevel = zoomLevel;
        }

        public override string ToString()
        {
            return GetRelativePath();
        }

        public TileCoordinates GetParent()
        {
            return new TileCoordinates(ZoomLevel - 1, X / 2, Y / 2);
        }

        public static TileCoordinates GetTileCoordinates(double x, double y, double resolution, bool reverseIntersectionPolicy, TiledVectorLayer layer)
        {
            var z = layer.GetZForResolution(resolution);
            return GetTileCoordinates(layer, x, y, resolution, reverseIntersectionPolicy, z);
        }

        public static TileCoordinates GetTileCoordinates(TiledVectorLayer layer, double x, double y, double resolution,
            bool reverseIntersectionPolicy, int z)
        {
            var scale = resolution / layer.GetResolution(z);
            var origin = layer.GetOrigin(z);

            return GetTileCoordForXyAndResolution(x, y, resolution, reverseIntersectionPolicy, origin, scale, z);
        }

        private static TileCoordinates GetTileCoordForXyAndResolution(double x, double y, double resolution,
            bool reverseIntersectionPolicy, Point origin, double scale, int z)
        {
            var adjustX = reverseIntersectionPolicy ? 0.5 : 0;
            var adjustY = reverseIntersectionPolicy ? 0 : 0.5;
            var xFromOrigin = Math.Floor((x - origin.X) / resolution + adjustX);
            var yFromOrigin = Math.Floor((y - origin.Y) / resolution + adjustY);
            var tileCoordX = scale * xFromOrigin / TiledVectorLayer.TileSize;
            var tileCoordY = scale * yFromOrigin / TiledVectorLayer.TileSize;

            if (reverseIntersectionPolicy)
            {
                tileCoordX = Math.Ceiling(tileCoordX) - 1;
                tileCoordY = Math.Ceiling(tileCoordY) - 1;
            }
            else
            {
                tileCoordX = Math.Floor(tileCoordX);
                tileCoordY = Math.Floor(tileCoordY);
            }

            return new TileCoordinates(z, (int)tileCoordX, (int)tileCoordY);
        }

        public BoundingBox GetExtent(TiledVectorLayer layer)
        {
            var origin = layer.GetOrigin(ZoomLevel);
            double resolution = layer.GetResolution(ZoomLevel);
            double minX = origin.X + X * TiledVectorLayer.TileSize * resolution;
            double minY = origin.Y + Y * TiledVectorLayer.TileSize * resolution;
            double maxX = minX + TiledVectorLayer.TileSize * resolution;
            double maxY = minY + TiledVectorLayer.TileSize * resolution;
            return new BoundingBox(minX, maxY, maxX, minY);
        }

        public static TileCoordinates FromRelativePath(string key)
        {
            string[] parts = key.Split('/');
            return new TileCoordinates(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

        public string GetRelativePath()
        {
            return $"{ZoomLevel}/{X}/{Y}";
        }
    }
}