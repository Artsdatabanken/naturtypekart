using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common.Rutenett;
using GeoJSON.Net.CoordinateReferenceSystem;
using Microsoft.SqlServer.Types;
using Nin.Områder;
using Nin.Types.MsSql;
using Feature = GeoJSON.Net.Feature.Feature;

namespace Nin.GeoJson
{
    public static class GeoJsonConverter
    {
        public static string NatureAreasToGeoJson(Collection<NatureArea> natureAreas, bool addId)
        {
            int featureCollectionEpsg = -1;

            if (natureAreas.Count > 0)
                featureCollectionEpsg = natureAreas[0].Area.STSrid.Value;

            for (int i = 1; i < natureAreas.Count; ++i)
            {
                if (featureCollectionEpsg == natureAreas[i].Area.STSrid.Value) continue;

                featureCollectionEpsg = -1;
                break;
            }

            var features = new List<Feature>();

            foreach (var natureArea in natureAreas)
            {
                var properties = new Dictionary<string, object>();
                if (natureArea.Parameters.Count == 1)
                    properties["ColorCode"] = natureArea.Parameters[0].Code;
                else if (natureArea.Parameters.Count > 1)
                    properties["ColorCode"] = "MOSAIC";

                string id = null;
                if (addId)
                    id = natureArea.UniqueId.LocalId.ToString();

                var feature = CreateFeature(natureArea.Area, properties, id, featureCollectionEpsg);
                features.Add(feature);
            }

            var featureCollection = new GeoJSON.Net.Feature.FeatureCollection(features);
            if (featureCollectionEpsg != -1)
                featureCollection.CRS = new NamedCRS("EPSG:" + featureCollectionEpsg);

            return GeoJsonWriter.ToGeoJson(featureCollection);
        }

        public static string AreasToGeoJson(Collection<Area> naturområder, bool addAreaLayer)
        {
            int featureCollectionEpsg = -1;

            // Assuming that all naturområder have the same projection.
            if (naturområder.Count > 0)
                featureCollectionEpsg = naturområder[0].Geometry.STSrid.Value;

            var features = new List<Feature>();

            foreach (var naturområde in naturområder)
            {
                var feature = AreaToGeoJson(addAreaLayer, naturområde, featureCollectionEpsg);
                features.Add(feature);
            }

            var featureCollection = new GeoJSON.Net.Feature.FeatureCollection(features);

            if (featureCollectionEpsg != -1)
                featureCollection.CRS = new NamedCRS("EPSG:" + featureCollectionEpsg);

            return GeoJsonWriter.ToGeoJson(featureCollection);
        }

        private static Feature AreaToGeoJson(bool addAreaLayer, Area area, int featureCollectionEpsg)
        {
            var properties = new Dictionary<string, object>
            {
                ["type"] = area.Type.ToString(),
                ["name"] = area.Name
            };

            if (addAreaLayer)
                properties["value"] = area.Value;

            var feature = CreateFeature(area.Geometry, properties, area.Number.ToString(), featureCollectionEpsg);
            return feature;
        }

        public static string GridToGeoJson(Grid grid, bool addGridLayer)
        {
            int featureCollectionEpsg = -1;

            // Assuming that all grid cells have the same projection.
            if (grid.Cells.Count > 0)
                featureCollectionEpsg = grid.Cells[0].Geometry.STSrid.Value;

            Dictionary<string, object> properties = null;
            var features = new List<Feature>();

            foreach (var cell in grid.Cells)
            {
                if (addGridLayer)
                {
                    properties = new Dictionary<string, object> {{"value", cell.Value}};
                }

                var feature = CreateFeature(cell.Geometry, properties, cell.CellId, featureCollectionEpsg);

                features.Add(feature);
            }

            var featureCollection = new GeoJSON.Net.Feature.FeatureCollection(features);

            if (featureCollectionEpsg != -1)
                featureCollection.CRS = new NamedCRS("EPSG:" + featureCollectionEpsg);

            return GeoJsonWriter.ToGeoJson(featureCollection);
        }

        private static Feature CreateFeature(SqlGeometry sqlGeometry, Dictionary<string, object> properties, string id, int featureCollectionEpsg)
        {
            var feature = new Feature(GeoJsonGeometry.FromSqlGeometry(sqlGeometry), properties, id);

            if (featureCollectionEpsg == -1 && sqlGeometry.STSrid.Value != 0)
                feature.CRS = new NamedCRS("EPSG:" + sqlGeometry.STSrid.Value);
            else
                feature.CRS = null;

            return feature;
        }
    }
}