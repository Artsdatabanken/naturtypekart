using System;
using Common;
using DotSpatial.Topology;
using Nin.Common.Map.Tiles.Stores;
using Nin.Configuration;
using Nin.Diagnostic;
using Nin.Map.Tiles;
using Nin.Map.Tiles.Stores;
using Nin.Map.Tiles.Vectors;
using Nin.Områder;
using AreaLayerValues = Nin.IO.SqlServer.AreaLayerValues;
using SqlServer = Nin.IO.SqlServer.SqlServer;

namespace Nin.Tasks
{
    public class TileAreaTask : Task
    {
        public AreaType AreaType;
        public int Number;
        public string Affected;
        public AreaLayerValues Values;
        private readonly string MapLayerName;

        public TileAreaTask()
        {
        }

        public TileAreaTask(AreaType areaType, int number, string mapLayer)
        {
            MapLayerName = mapLayer;
            AreaType = areaType;
            Number = number;
        }

        public override void Execute(NinServiceContext context)
        {
            Values = SqlServer.GetAreaLayerValues(AreaType, Number);

            var layer = Config.Settings.Map.FindLayer(MapLayerName);
            var store = new Cache<VectorQuadTile>(new DiskTileStore(layer));
            var tiler = new VectorTiler(store, layer);

            var areas = SqlServer.GetAreas(AreaType, 0, Number);
            ZoomFactor zf = layer.GetZoomFactors(AreaType);
            string allAffected = "";
            foreach (var area in areas)
            {
                for (int zoom = zf.Minimum; zoom <= zf.Maximum; zoom++)
                {
                    var distanceTolerance = layer.SimplificationToleranceAtZoom0 * Math.Pow(0.25, zoom);
                    var fullRes = DotSpatialGeometry.From(area.Geometry);
                    Geometry simpler = VectorTiler.Simplify(fullRes, distanceTolerance);
                    if (simpler.IsEmpty) continue;
                    var område = Map.Tiles.Geometri.Område.Fra(area);
                    Log.v("TILE", $"Area type {område.Type} #{område.Number}: {fullRes.Coordinates.Count} => {simpler.Coordinates.Count} (zoom {zoom})");
                    var affected = tiler.Update(område, simpler, zoom);
                    if (allAffected.Length > 0) allAffected += ",";
                    allAffected += affected;
                }
            }
            Affected = allAffected;
        }
    }
}
