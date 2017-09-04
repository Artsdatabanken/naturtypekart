using System.Collections.Generic;
using DotSpatial.Topology;
using Nin.Common.Map.Tiles.Stores;
using Nin.Map.Layers;
using Nin.Map.Tiles.Vectors;

namespace Nin.Map.Tiles
{
    public class VectorTiler
    {
        public TileRange Update(Geometri.Område area, Geometry geometry, int zoomLevel)
        {
            var tiles = GetTileRange(zoomLevel, (Envelope)geometry.Envelope);
            foreach (var tile in tiles)
                Update(tile, area, geometry);
            return tiles;
        }

        private void Update(TileCoordinates tile, Geometri.Område area, Geometry geometry)
        {
            VectorQuadTile qt = tileStore.Load(tile.GetRelativePath());
            if (!qt.ClipAndAdd(area, geometry, layer.TileOverlapRatio))
            {
                //throw new Exception("Update yields no result for tile " + tile.ToString());
                return;
            }
            Save(qt);
        }

        private void Save(VectorQuadTile qt)
        {
            var parent = qt.TileCoordinates.GetParent();
            dirty.Add(parent);
            tileStore.Save(qt.TileCoordinates.GetRelativePath(), qt);
        }

        public TileRange GetTileRange(int zoomLevel, Envelope bb)
        {
            var intersection = bb.Intersection(layer.GetEnvelope());
            return TileRange.GetTileRangeForExtentAndZ(intersection, zoomLevel, layer);
        }

        public static Geometry Simplify(Geometry geometry, double distanceTolerance)
        {
            var simplify = (DotSpatial.Topology.Geometry)DotSpatial.Topology.Simplify.DouglasPeuckerSimplifier.Simplify(geometry, distanceTolerance);
            if (simplify.GeometryType != "GeometryCollection") return simplify;

            // HACK. TODO. DouglasPeuckerSimplifier splits and changes geometrytypes.  
            for (int i = 0; i < simplify.NumGeometries; i++)
            {
                var n = simplify.GetBasicGeometryN(i);
                if (n.GeometryType == "Polygon") return (Polygon)n;
            }
            return (Geometry) Polygon.Empty;
            //var bg = simplify.GetBasicGeometryN(0);
            //if (bg is LineString) return (LineString) bg;
            //throw new Exception("How unfortunate.");
        }

        public VectorTiler(IPersistStuff<VectorQuadTile> testTileStore, TiledVectorLayer layer)
        {
            this.layer = layer;
            tileStore = testTileStore;
        }

        readonly HashSet<TileCoordinates> dirty = new HashSet<TileCoordinates>();
        private readonly TiledVectorLayer layer;
        private readonly IPersistStuff<VectorQuadTile> tileStore;
    }
}