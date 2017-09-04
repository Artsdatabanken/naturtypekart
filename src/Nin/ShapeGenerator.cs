using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using DotSpatial.Data;
using DotSpatial.Projections;
using Types;
using DescriptionVariable = Nin.Types.MsSql.DescriptionVariable;
using NatureArea = Nin.Types.MsSql.NatureArea;
using NatureAreaType = Nin.Types.MsSql.NatureAreaType;

namespace Common
{
    public static class ShapeGenerator 
    {
        public static MemoryStream GenerateShapeFile(Collection<NatureArea> natureAreas, int epsgCode)
        {
            // Return empty memory stream if there are no nature areas or no epsg code:
            if (natureAreas.Count == 0 || epsgCode < 0)
                return new MemoryStream();

            // Define a new set of features and set projection:
            FeatureSet featureSets = new FeatureSet {Projection = ProjectionInfo.FromEpsgCode(epsgCode)};

            // Add data columns
            featureSets.DataTable.Columns.Add(new DataColumn("LocalId", typeof (string)));
            featureSets.DataTable.Columns.Add(new DataColumn("Nivå", typeof (string)));
            featureSets.DataTable.Columns.Add(new DataColumn("NiN", typeof (string)));

            // Create geometry objects:
            foreach (var natureArea in natureAreas)
            {
                if (natureArea.Area == null) continue;
                IFeature feature = featureSets.AddFeature(DotSpatialGeometry.GetGeometry(natureArea.Area));

                // Adding values for the data columns
                feature.DataRow.BeginEdit();
                feature.DataRow["LocalId"] = natureArea.UniqueId.LocalId.ToString();
                feature.DataRow["Nivå"] = Naturnivå.TilNavn(natureArea.Nivå);
                feature.DataRow["NiN"] = FormatNatureAreaTypes(natureArea.Parameters);
                feature.DataRow.EndEdit();
            }

            // Create temporary directory:
            string tempDirectoryPath = GetTempDirectoryPath();
            
            // Save the feature set:
            featureSets.SaveAs(tempDirectoryPath + "\\TempShapeFiles\\data.shp", true);

            // Zip all shape files:
            ZipFile.CreateFromDirectory(tempDirectoryPath + "\\TempShapeFiles", tempDirectoryPath + "\\data.zip");

            // Read the zip file:
            var bytes = File.ReadAllBytes(tempDirectoryPath + "\\data.zip");

            // Delete the temporary directory:
            Directory.Delete(tempDirectoryPath, true);

            return new MemoryStream(bytes);
        }

        private static string GetTempDirectoryPath()
        {
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectoryPath);
            return tempDirectoryPath;
        }

        private static string FormatNatureAreaTypes(IEnumerable<Parameter> parameters)
        {
            string natureTypes = "";
            string descriptionVariables = "";

            foreach (var parameter in parameters)
            {
                if (parameter.GetType() == typeof(NatureAreaType))
                {
                    if (!string.IsNullOrEmpty(natureTypes)) natureTypes += "\n";
                    var natureAreaType = (NatureAreaType)parameter;
                    natureTypes += natureAreaType.Code;
                    if (natureAreaType.AdditionalVariables.Count <= 0) continue;

                    natureTypes += ": ";
                    for (int i = 0; i < natureAreaType.AdditionalVariables.Count; ++i)
                    {
                        natureTypes += natureAreaType.AdditionalVariables[i].Code;
                        if (i == (natureAreaType.AdditionalVariables.Count - 1)) continue;
                        natureTypes += ", ";
                    }
                }
                else if (parameter.GetType() == typeof(DescriptionVariable))
                {
                    if (!string.IsNullOrEmpty(descriptionVariables)) descriptionVariables += "\n";
                    descriptionVariables += parameter.Code;
                }
            }

            var natureAreaTypes = "";

            if (!string.IsNullOrEmpty(natureTypes) && !string.IsNullOrEmpty(descriptionVariables))
                natureAreaTypes = natureTypes + "\n" + descriptionVariables;
            else if (!string.IsNullOrEmpty(natureTypes))
                natureAreaTypes = natureTypes;
            else if (!string.IsNullOrEmpty(descriptionVariables))
                natureAreaTypes = descriptionVariables;

            return natureAreaTypes;
        }
    }
}
