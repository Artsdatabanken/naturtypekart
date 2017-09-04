using System;
using System.Collections;
using System.Collections.Generic;
using DotSpatial.Topology;
using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Map.Layers;
using Point = Nin.Map.Tiles.Geometri.Point;

namespace Nin.Map.Tiles
{
    public class TileRange : IEnumerable<TileCoordinates>
    {
        public int ZoomLevel;
        public int minX;
        public int maxX;
        public int minY;
        public int maxY;

        public TileRange(int zoomLevel, int minX, int maxX, int minY, int maxY)
        {
            if (minX > maxX)
            {
                int tmp = minX;
                minX = maxX;
                maxX = tmp;
            }
            if (minY > maxY)
            {
                int tmp = minY;
                minY = maxY;
                maxY = tmp;
            }
            if (minX > maxX || minY > maxY) throw new Exception($"Invalid tile range ({minX},{minY})-({maxX},{maxY})");
            ZoomLevel = zoomLevel;
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }

        public BoundingBox GetExtent(TiledVectorLayer layer)
        {
            Point origin = layer.GetOrigin(ZoomLevel);
            double resolution = layer.GetResolution(ZoomLevel);
            double rminX = origin.X + minX * TiledVectorLayer.TileSize * resolution;
            double rmaxX = origin.X + (maxX + 1) * TiledVectorLayer.TileSize * resolution;
            double rminY = origin.Y + minY * TiledVectorLayer.TileSize * resolution;
            double rmaxY = origin.Y + (maxY + 1) * TiledVectorLayer.TileSize * resolution;
            return new BoundingBox(rminX, rmaxY, rmaxX, rminY);
        }

        public static TileRange GetTileRangeForExtentAndResolution(IEnvelope extent, double resolution, TiledVectorLayer layer)
        {
            var z = layer.GetZForResolution(resolution);
            var ll = TileCoordinates.GetTileCoordinates(layer, extent.Minimum.X, extent.Minimum.Y, resolution, false, z);
            var ur = TileCoordinates.GetTileCoordinates(layer, extent.Maximum.X, extent.Maximum.Y, resolution, true, z);
            return new TileRange(z, ll.X, ur.X, ll.Y, ur.Y);
        }

        public static TileRange GetTileRangeForExtentAndZ(IEnvelope extent, int z, TiledVectorLayer layer)
        {
            var resolution = layer.GetResolution(z);
            return GetTileRangeForExtentAndResolution(extent, resolution, layer);
        }

        public override string ToString()
        {
            if (minX == maxX && minY == maxY)
                return $"{ZoomLevel}: [{minX},{minY}]";
            return $"{ZoomLevel}: [[{minX},{minY}],[{maxX},{maxY}]]";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TileCoordinates> GetEnumerator()
        {
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    yield return new TileCoordinates(ZoomLevel, x, y);
        }
    }
}