using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Common;
using GeoJSON.Net.CoordinateReferenceSystem;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Nin.Common.Map.Tiles.Stores;
using Nin.Configuration;
using Nin.GeoJson;
using Nin.Map.Layers;
using Nin.Map.Tiles.geojson;
using Nin.Map.Tiles.Geometri;
using Nin.Map.Tiles.Vectors;
using Nin.Områder;
using Feature = GeoJSON.Net.Feature.Feature;
using FeatureCollection = GeoJSON.Net.Feature.FeatureCollection;

namespace Nin.Map.Tiles.Stores
{
    /// <summary>
    /// for /R %f in (*.geojson) do C:\Temp\ClassLibrary1\node_modules\.bin\topojson.cmd -o "%~dpnf.topojson" "%f"
    /// </summary>
    public class DiskTileStore : IPersistStuff<VectorQuadTile>
    {
        public void Save(string key, VectorQuadTile tile)
        {
            var ro = new FeatureCollection();
            var crs = new NamedCRS("EPSG:" + Config.Settings.Map.SpatialReferenceSystemIdentifier);
            ro.CRS = crs;

            foreach (var omg in tile.Områder)
            {
                var område = omg.Område;
                IGeometryObject geometry = GeoJsonGeometry.FromDotSpatial(omg.Geometry);
                var props = new Dictionary<string, object>
                {
                    {"category", område.Category},
                    {"name", område.Name},
                    {"number", område.Number},
                    {"value", område.Value},
                    {"type", område.Type.ToString()},
                    {"kind", område.kind}
                };
                var feature = new Feature(geometry, props) {Id = område.AreaId.ToString()};
                var fp = new FeatureProperties {nin = område.kind};
                if (område.Number == 0) throw new Exception("Område mangler nummer.");
                ro.Features.Add(feature);
            }

            var fullPath = GetFullPath(key);
            var fullPathSingleDir = GetFullPath(key.Replace("/", "_"));

            var settings = GetJsonSerializerSettings();

            File.WriteAllText(fullPath, JsonConvert.SerializeObject(ro, settings));
            //File.WriteAllText(fullPathSingleDir, JsonConvert.SerializeObject(ro, settings));

            //var serializer = new DataContractJsonSerializer(typeof(VectorQuadTile), CreateDataContractJsonSerializerSettings());
            //using (Stream stream = File.Create(fullPath + ".adf"))
            //    serializer.WriteObject(stream, tile);
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = Formatting.None,
                Converters = new List<JsonConverter> { new JsonFloatConverter() }
            };
            return settings;
        }

        public VectorQuadTile Load(string key)
        {
            var qt = new VectorQuadTile(TileCoordinates.FromRelativePath(key), layer);

            var fullPath = GetFullPath(key);
            if (!File.Exists(fullPath))
                return qt;

            string json = File.ReadAllText(fullPath);
            var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json, GetJsonSerializerSettings());
            foreach (Feature feature in featureCollection.Features)
            {
                var geometry = DotSpatialGeometry.FromGeoJson(feature.Geometry);
                var areaType = (AreaType)Enum.Parse(typeof(AreaType), feature.Properties["type"].ToString());
                var areaId = int.Parse(feature.Id, CultureInfo.InvariantCulture);
                var område = new Område(areaId, areaType)
                {
                    Category = ReadProp(feature, "category"),
                    Name = ReadProp(feature, "name"),
                    Number = int.Parse(feature.Properties["number"].ToString(), CultureInfo.InvariantCulture),
                    Value = ReadProp(feature, "value"),
                    kind = ReadProp(feature, "kind")
                };

                qt.Områder.Add(new OmrådeMedGeometry(område, geometry));
            }
            return qt;
            //var serializer = new DataContractJsonSerializer(typeof(VectorQuadTile), CreateDataContractJsonSerializerSettings());
            //using (Stream s = File.OpenRead(fullPath))
            //    return (VectorQuadTile)serializer.ReadObject(s);
        }

        private static string ReadProp(Feature feature, string key)
        {
            object r;
            if (!feature.Properties.TryGetValue(key, out r))
                return "";
            return r?.ToString();
        }

        private string GetFullPath(string key)
        {
            var fullPath = Path.Combine(storagePath,
                key + ".geojson");
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            return fullPath;
        }

        public void Wipe()
        {
            var di = new DirectoryInfo(storagePath);
            if (!di.Exists) return;

            foreach (var file in di.GetFiles())
                file.Delete();
            foreach (var dir in di.GetDirectories())
                dir.Delete(true);
        }

        private static void CreateDirectory(string mapLayersPath)
        {
            try
            {
                Directory.CreateDirectory(mapLayersPath);
            }
            catch (Exception caught)
            {
                throw new Exception($"Create directory '{mapLayersPath}' failed.", caught);
            }
        }

        public DiskTileStore(TiledVectorLayer layer)
        {
            this.layer = layer;

            var mapsPath = Config.Settings.Map.MapLayersPath;
            storagePath = Path.Combine(mapsPath, layer.Name);
            CreateDirectory(storagePath);
        }

        private readonly TiledVectorLayer layer;
        private readonly string storagePath;
    }
}