using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using Common;
using DotSpatial.Data;
using Microsoft.SqlServer.Types;
using Nin;
using Nin.Configuration;
using Nin.Types.MsSql;

namespace Test.Integration.Nin.Common
{
    public static class TestDataDelivery
    {
        public static Dataleveranse Create(string shpFilepath, int shpSpatialReference, int offset, int count)
        {
            const string fylke = "Sør-Trøndelag";
            const string knr = "1648";
            Collection<NatureArea> natureAreas = GenerateNatureAreasFromShapeFile(shpFilepath, shpSpatialReference, knr, offset, count);
            if (natureAreas.Count <= 0) return null;
            foreach (var area in natureAreas)
            {
                area.Institution = "BOCI";
                area.Description = "boci description";
            }

            SqlGeometry metadataArea = CreateMetadataArea(fylke, shpSpatialReference);

            HashSet<string> variableDefinitions = new HashSet<string>();

            foreach (var natureArea in natureAreas)
            {
                metadataArea = metadataArea.STUnion(natureArea.Area);
                foreach (var parameter in natureArea.Parameters)
                {
                    variableDefinitions.Add(parameter.Code);
                    foreach (var variable in ((NatureAreaType) parameter).AdditionalVariables)
                        variableDefinitions.Add(variable.Code);
                }
            }

            var dataDelivery = new Dataleveranse {Id = "-1"};
            var rangeString = "" + offset + "-" + (offset + count - 1);
            dataDelivery.Name = Path.GetFileNameWithoutExtension(shpFilepath) + " ("+rangeString + ")";
            dataDelivery.Created = DateTime.Now;
            dataDelivery.DeliveryDate = DateTime.Now;
            dataDelivery.Operator = natureAreas[0].Surveyer;

            var metadata = new Metadata {UniqueId = new Identification()};

            var numberString = offset.ToString();
            var guid = "00000000-0000-" + knr + "-0000-";
            for (int i = 0; i < (12 - numberString.Length); ++i)
                guid += "0";

            metadata.UniqueId.LocalId = new Guid(guid + numberString);
            metadata.UniqueId.NameSpace = "NBIC";
            metadata.UniqueId.VersionId = "1.0";

            metadata.Program = "Program name";
            metadata.ProjectName = "Project name";
            metadata.Contractor = natureAreas[0].Surveyer;
            metadata.Owner = natureAreas[0].Surveyer;
            metadata.SurveyedFrom = DateTime.Now;
            metadata.SurveyedTo = DateTime.Now;
            metadata.SurveyScale = "1:50000";
            metadata.Area = metadataArea;
            metadata.Quality = new Quality
            {
                MeasuringMethod = "10",
                Accuracy = 5,
                Visibility = "0",
                MeasuringMethodHeight = "10",
                AccuracyHeight = 6,
                MaxDeviation = 7
            };

            metadata.NatureAreas = natureAreas;

            foreach (var variableDefinition in variableDefinitions)
            {
                metadata.VariabelDefinitions.Add(new NinStandardVariabel
                {
                    VariableDefinition = new Code
                    {
                        Value = variableDefinition,
                        Registry = "NiN",
                        Version = "2.0"
                    }
                });
            }

            dataDelivery.Metadata = metadata;
            return dataDelivery;
        }

        private static SqlGeometry CreateMetadataArea(string fylke, int targetSrs)
        {
            SqlGeometry metadataArea = new SqlGeometry();
            const int metadatapolygonEpsg = 3857;
            const string metadatapolygon =
                "POLYGON((1102527.6959853824 9005963.546616036, 1237056.8657672927 9005963.546616036, 1237056.8657672927 9137740.98337968, 1102527.6959853824 9137740.98337968, 1102527.6959853824 9005963.546616036))";
            if (metadatapolygon.Length == 0)
            {
                var path = FileLocator.FindFileInTree(@"Data\area\fylker.txt");
                var areas = AreaCollection.FromGeoJson(File.ReadAllText(path), 4326);

                foreach (var area in areas)
                    if (area.Name.Equals(fylke))
                        metadataArea = area.Geometry;
            }
            else
            {
                metadataArea = SqlGeometry.Parse(metadatapolygon);
                metadataArea.STSrid = metadatapolygonEpsg;
                metadataArea =MapProjection.Reproject(metadataArea, targetSrs);
            }
            return metadataArea;
        }

        private static Collection<NatureArea> GenerateNatureAreasFromShapeFile(string shapeFilePath, int epsgCode, string knr, int startFeature, int featureCount)
        {
            var natureAreas = new Collection<NatureArea>();
            shapeFilePath = FileLocator.FindFileInTree(shapeFilePath);
            Shapefile shapeFile = Shapefile.OpenFile(shapeFilePath);
            var endFeature = startFeature + featureCount < shapeFile.Features.Count
                ? startFeature + featureCount
                : shapeFile.Features.Count;
            for (var number = startFeature; number < endFeature; number++)
            {
                var feature = shapeFile.Features[number];
                var area = new NatureArea {UniqueId = new Identification()};

                var numberString = number.ToString();
                var guid = "00000000-0000-0000-" + knr + "-";

                for (int i = 0; i < 12 - numberString.Length; ++i)
                    guid += "0";

                area.UniqueId.LocalId = new Guid(guid + numberString);

                area.UniqueId.NameSpace = "NBIC";
                area.UniqueId.VersionId = "1.0";

                area.Version = "1";
                area.Nivå = Types.NatureLevel.Natursystem;

                area.Area = SqlGeometry.STGeomFromText(new SqlChars(feature.BasicGeometry.ToString()), epsgCode);

                var natureAreaType = new NatureAreaType();
                const int rowOffset = 0; // 1 for melhus
                natureAreaType.Code = (string)feature.DataRow[rowOffset + 4];
                natureAreaType.Share = 1;
                if (feature.DataRow[rowOffset + 3] != DBNull.Value)
                {
                    var descriptionVariable = new DescriptionVariable();
                    var descriptionVariableParts = ((string)feature.DataRow[rowOffset + 3]).Split('_');
                    descriptionVariable.Code = descriptionVariableParts[0];
                    descriptionVariable.Value = descriptionVariableParts[1];
                    natureAreaType.AdditionalVariables.Add(descriptionVariable);
                }
                area.Parameters.Add(natureAreaType);

                area.Surveyed = DateTime.Parse(feature.DataRow[rowOffset + 5].ToString());
                area.Surveyer = new Contact {Company = (string) feature.DataRow[rowOffset + 8]};
                natureAreas.Add(area);
            }
            return natureAreas;
        }
    }
}