using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Microsoft.SqlServer.Types;
using Nin.Configuration;
using Nin.Naturtyper;
using Nin.Types.MsSql;
using Types;
using DescriptionVariable = Nin.Types.MsSql.DescriptionVariable;
using NatureAreaType = Nin.Types.MsSql.NatureAreaType;

namespace Nin.IO.Excel
{
    public class ExcelGenerator 
    {
        private readonly MapProjection reproject;
        private readonly Naturetypekodetre naturtypeKodetre;

        public ExcelGenerator(Naturetypekodetre allCodes)
        {
            naturtypeKodetre = allCodes;
            reproject = new MapProjection(Config.Settings.Export.ExcelSpatialReferenceSystemIdentifier);
        }

        public MemoryStream GenerateXlsxStream(Collection<NatureAreaExport> natureAreas)
        {
            var descriptionVariableColumns = GetDistinctDescriptionVariables(natureAreas);

            var workBook = new XLWorkbook();
            var sheet = workBook.AddWorksheet("Naturtypekart");
            sheet.Name = "Naturtypekart";

            int columnNumber = 0;
            int rowNumber = 0;

            sheet.Cell(1, 1).Value = "asdf";
            sheet.Cell(++rowNumber, ++columnNumber).Value= "Id";
            sheet.Cell(rowNumber, ++columnNumber).Value = "Naturnivå";
            sheet.Cell(rowNumber, ++columnNumber).Value = "Hovedtypegruppe";
            sheet.Cell(rowNumber, ++columnNumber).Value = "Hovedtype";
            sheet.Cell(rowNumber, ++columnNumber).Value = "Grunntype";
            sheet.Cell(rowNumber, ++columnNumber).Value = "Mosaikk";
            sheet.Cell(rowNumber, ++columnNumber).Value = "Målestokk";


            foreach (var descriptionVariableColumn in descriptionVariableColumns)
            {
                sheet.Cell(rowNumber, ++columnNumber).Value = descriptionVariableColumn;
            }

            sheet.Cell(rowNumber, ++columnNumber).Value =  "Kartlagt dato";
            sheet.Cell(rowNumber, ++columnNumber).Value =  "Datakilde";
            sheet.Cell(rowNumber, ++columnNumber).Value =  "Program";
            sheet.Cell(rowNumber, ++columnNumber).Value =  "Størrelse (m²)";

            sheet.Row(rowNumber).Style.Font.Bold = true;
            sheet.Row(rowNumber).Style.Fill.SetPatternColor(XLColor.LightGray);

            foreach (var natureArea in natureAreas)
            {
                SqlGeometry area = null;
                if (natureArea.Area != null)
                    area = reproject.Reproject(natureArea.Area);

                foreach (var parameter in natureArea.Parameters)
                {
                    ++rowNumber;
                    sheet.Cell(rowNumber, 1).Value = natureArea.UniqueId.LocalId.ToString();
                    sheet.Cell(rowNumber, 2).Value = Naturnivå.TilNavn(natureArea.Nivå);

                    sheet.Cell(rowNumber, 6).Value = natureArea.Parameters.Count > 1 ? "Ja" : "Nei";
                    sheet.Cell(rowNumber, 7).Value = natureArea.MetadataSurveyScale;

                    if (parameter.GetType() == typeof(NatureAreaType))
                    {
                        var natureAreaType = (NatureAreaType)parameter;

                        var ninCode = naturtypeKodetre.HentFraKode(natureAreaType.Code);
                        if (ninCode != null && ninCode.ParentCodeItems.Count == 1)
                        {
                            sheet.Cell(rowNumber, 4).Value = ninCode.Name;
                        }
                        else if (ninCode != null && ninCode.ParentCodeItems.Count == 2)
                        {
                            sheet.Cell(rowNumber, 4).Value =  ninCode.ParentCodeItems[1].Name;
                        }
                        else if (ninCode != null && ninCode.ParentCodeItems.Count == 3)
                        {
                            sheet.Cell(rowNumber, 3).Value =  ninCode.ParentCodeItems[1].Name;
                            sheet.Cell(rowNumber, 4).Value =  ninCode.ParentCodeItems[2].Name;
                        }

                        sheet.Cell(rowNumber, 5).Value =  natureAreaType.Code;

                        foreach (var additionalVariable in natureAreaType.AdditionalVariables)
                        {
                            int columnIndex = descriptionVariableColumns.IndexOf(additionalVariable.Code);

                            double additionalVariableValueDouble;
                            int additionalVariableValueInt;
                            if (double.TryParse(additionalVariable.Value, out additionalVariableValueDouble))
                            {
                                sheet.Cell(rowNumber, columnIndex + 8).Value = additionalVariableValueDouble;
                            }
                            else if (int.TryParse(additionalVariable.Value, out additionalVariableValueInt))
                            {
                                sheet.Cell(rowNumber, columnIndex + 8).Value = additionalVariableValueInt;
                            }
                            else
                            {
                                sheet.Cell(rowNumber, columnIndex + 8).Value =  additionalVariable.Value;
                            }

                        }
                    }
                    else if (parameter.GetType() == typeof(DescriptionVariable))
                    {
                        var descriptionVariable = (DescriptionVariable)parameter;
                        int columnIndex = descriptionVariableColumns.IndexOf(descriptionVariable.Code);

                        double descriptionVariableValueDouble;
                        int descriptionVariableValueInt;
                        if (double.TryParse(descriptionVariable.Value, out descriptionVariableValueDouble))
                        {
                            sheet.Cell(rowNumber, columnIndex + 8).Value = descriptionVariableValueDouble;
                        }
                        else if (int.TryParse(descriptionVariable.Value, out descriptionVariableValueInt))
                        {
                            sheet.Cell(rowNumber, columnIndex + 8).Value =  descriptionVariableValueInt;
                        }
                        else
                        {
                            sheet.Cell(rowNumber, columnIndex + 8).Value = descriptionVariable.Value;
                        }
                    }

                    sheet.Cell(rowNumber, 8 + descriptionVariableColumns.Count).Value = natureArea.Surveyed?.ToShortDateString() ?? string.Empty;
                    sheet.Cell(rowNumber, 9 + descriptionVariableColumns.Count).Value = natureArea.Institution;
                    sheet.Cell(rowNumber, 10 + descriptionVariableColumns.Count).Value = natureArea.MetadataProgram;

                    if (area != null)
                    {
                        sheet.Cell(rowNumber, 11 + descriptionVariableColumns.Count).Value = 
                            Math.Round(area.STArea().Value, 2);
                    }
                }
            }

            sheet.AutoFilter.Set(sheet.Range(1, 1, columnNumber, rowNumber));

            sheet.Columns().AdjustToContents();

            MemoryStream xlsxStream = new MemoryStream();
            workBook.SaveAs(xlsxStream);
            xlsxStream.Position = 0;

            return xlsxStream;
        }

        private static List<string> GetDistinctDescriptionVariables(IEnumerable<NatureAreaExport> natureAreas)
        {
            var distinctDescriptionVariables = new HashSet<string>();
            foreach (var natureArea in natureAreas)
                foreach (var parameter in natureArea.Parameters)
                    if (parameter.GetType() == typeof(NatureAreaType))
                    {
                        var natureAreaType = (NatureAreaType) parameter;
                        foreach (var additionalVariable in natureAreaType.AdditionalVariables)
                            distinctDescriptionVariables.Add(additionalVariable.Code);
                    }
                    else if (parameter.GetType() == typeof(DescriptionVariable))
                        distinctDescriptionVariables.Add(((DescriptionVariable) parameter).Code);

            return distinctDescriptionVariables.ToList();
        } 
    }
}
