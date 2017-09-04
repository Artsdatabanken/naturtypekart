using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using DotSpatial.Data;
using DotSpatial.Projections;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Nin.Common;
using Nin.Configuration;
using Nin.Områder;
using Feature = Nin.Områder.Feature;

namespace Nin
{
    public class AreaCollection : Collection<Area>
    {
        /// <summary>
        /// Convert features defined in json to NiN Areas.
        /// </summary>
        /// <param name="jsonAreas">The json file as a string</param>
        /// <param name="sourceEpsgCode">The epsg code for the json file</param>
        /// <returns>A collection of NiN Areas</returns>
        public static AreaCollection FromGeoJson(string jsonAreas, int sourceEpsgCode)
        {
            FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(jsonAreas);
            if (sourceEpsgCode == 0)
                sourceEpsgCode = featureCollection.GetEpsgCode();
            var areas = FromGeoJson(featureCollection, sourceEpsgCode);
            Reproject(areas);
            return areas;
        }

        /// <summary>
        /// Convert features defined in a shape file to Areas.
        /// </summary>
        /// <param name="shapeFilePath">Path to the shape file</param>
        /// <param name="areaType">area type</param>
        /// <param name="sourceEpsgCode"></param>
        private static AreaCollection FromShapeFile(string shapeFilePath, AreaType areaType, int sourceEpsgCode)
        {
            var areas = new AreaCollection();
            Shapefile shapeFile = Shapefile.OpenFile(shapeFilePath);
            shapeFile.Reproject(ProjectionInfo.FromEpsgCode(sourceEpsgCode));

            foreach (var feature in shapeFile.Features)
            {
                var area = new Area();
                var id = feature.DataRow[0].ToString();
                var number = int.Parse(id.Replace("VV", ""));
                area.Number = number;
                area.Name = feature.DataRow[1].ToString();
                area.Type = areaType;
                area.Geometry = SqlGeometry.STGeomFromText(new SqlChars(feature.BasicGeometry.ToString()),
                    Config.Settings.Map.SpatialReferenceSystemIdentifier);
                area.Category = feature.DataRow[3].ToString();
                areas.Add(area);
            }

            return areas;
        }

        private static void Reproject(IEnumerable<Area> areas)
        {
            var reproject = new MapProjection(Config.Settings.Map.SpatialReferenceSystemIdentifier);
            foreach (var area in areas)
                area.Geometry = reproject.Reproject(area.Geometry);
        }

        private static AreaCollection FromGeoJson(FeatureCollection jsonAreas, int sourceEpsgCode)
        {
            var areas = new AreaCollection();

            foreach (Feature t in jsonAreas.Features)
                areas.Add(JsonFeatureToArea(t, sourceEpsgCode));

            return areas;
        }

        private static Area JsonFeatureToArea(Feature jsonFeature, int sourceEpsgCode)
        {
            var area = new Area {Name = jsonFeature.Properties["navn"]};
            GjettOmrådetype(jsonFeature, area);

            var geometry = new SqlGeometryBuilder();
            geometry.SetSrid(sourceEpsgCode);
            geometry.BeginGeometry(OpenGisGeometryType.Polygon);

            var coordinates = jsonFeature.Geometry.Coordinates;
            foreach (var feature in coordinates)
            {
                for (int j = 0; j < feature.Count; ++j)
                {
                    var coordinate = feature[j];
                    if (j == 0) geometry.BeginFigure(coordinate[0], coordinate[1]);
                    else geometry.AddLine(coordinate[0], coordinate[1]);
                }
                geometry.EndFigure();
            }

            geometry.EndGeometry();
            area.Geometry = geometry.ConstructedGeometry.MakeValid();

            return area;
        }

        private static void GjettOmrådetype(Feature jsonFeature, Area area)
        {
            var properties = jsonFeature.Properties;
            if (properties.ContainsKey("objtype") && properties["objtype"] == "Fylke")
            {
                area.Type = AreaType.Fylke;
                area.Number = int.Parse(properties["fylkesnr"]);
            }
            else if (properties.ContainsKey("objtype") && properties["objtype"] == "Kommune")
            {
                area.Type = AreaType.Kommune;
                area.Number = int.Parse(properties["komm"]);
            }
            else
                throw new AreaConverterException("Unknown source file");
        }

        public static AreaCollection ImportAreasFromFile(string sourceFile, int sourceEpsgCode=0,
            AreaType areaType = AreaType.Undefined)
        {
            AreaCollection  areas;
            var extension = Path.GetExtension(sourceFile);
            switch (extension)
            {
                case ".geojson":
                    var countys = File.ReadAllText(sourceFile);
                    areas = FromGeoJson(countys, sourceEpsgCode);
                    break;
                case ".shp":
                    areas = FromShapeFile(sourceFile, areaType, sourceEpsgCode);
                    break;
                default:
                    throw new Exception("Ukjent format '" + extension + "'.");
            }
            return areas;
        }
    }
}
