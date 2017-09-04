using System.Collections.Generic;
using System.IO;
using Common;
using Newtonsoft.Json;
using Nin.Common.Map.Tiles;
using Nin.Configuration;
using Nin.Map.Layers;
using Nin.Map.Tiles;
using Nin.Map.Tiles.Geometri;
using Nin.Map.Tiles.Stores;
using Nin.Map.Tiles.Vectors;
using Nin.Områder;
using Nin.Tasks;
using FeatureCollection = GeoJSON.Net.Feature.FeatureCollection;

namespace Test.Integration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new TestSetup().RunBeforeAnyTests();

            TestTaskQueue.ProcessTask(new TileAreaTask(AreaType.Kommune, 0, "Nin"));
            TileNorway();
        }

        private static void TileNorway()
        {
            var layer = new TiledVectorLayer("test2", WebMercator.BoundingBox1, 39135.75848201024 * 4);
            var store = new DiskTileStore(layer);
            store.Wipe();
            var tiler = new VectorTiler(store, layer);
            var filename = FileLocator.FindFileInTree(@"data\norway.geojson");
            FeatureCollection norway = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText(filename));
            List<OmrådeMedGeometry> partOfNorway = new List<OmrådeMedGeometry>();
            foreach (var feature in norway.Features)
                partOfNorway.Add(new OmrådeMedGeometry(new Område(3581, AreaType.Land), DotSpatialGeometry.From(feature)));
            foreach (var omg in partOfNorway)
                omg.Område.Number = -5;
            for (int zoom = 0; zoom < 6; zoom++)
                foreach (OmrådeMedGeometry polygon in partOfNorway)
                    tiler.Update(polygon.Område, polygon.Geometry, zoom);
        }
    }
}