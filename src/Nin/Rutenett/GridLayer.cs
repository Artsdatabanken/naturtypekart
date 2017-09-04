using System;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Xml.Linq;
using ClosedXML.Excel;
using Microsoft.SqlServer.Types;
using Nin.Common;
using Nin.Common.Map.Geometric.Grids;
using Nin.Dataleveranser;
using Nin.Dataleveranser.Rutenett;
using Nin.Områder;
using Nin.Types.GridTypes;
using Nin.Types.MsSql;

namespace Nin.Rutenett
{
    public class GridLayerImpl : GridLayer
    {
        public GridLayerImpl()
        {
        }

        private GridLayerImpl(string name, RutenettType rutenettType) : base(name, rutenettType)
        {
        }

        public static GridLayerImpl FromExcelFile(string excelFilePath, RutenettType rutenettType, string name, int columnNumber)
        {
            var gridLayer = new GridLayerImpl(name, rutenettType);

            var workBook = new XLWorkbook(excelFilePath);
            var sheet = workBook.Worksheet(0);

            int endIndexRow = sheet.LastRow().RowNumber();
            for (int i = 2; i <= endIndexRow; ++i)
            {
                IXLRow row = sheet.Row(i);
                object value = row.Cell(columnNumber).Value;

                var cellId = row.Cell(1).Value;
                gridLayer.Cells.Add(new GridLayerCell { CellId = cellId.ToString(), Value = value.ToString() });
            }

            return gridLayer;
        }

        public static GridLayerImpl ParseXml(XDocument gridLayerXml)
        {
            var layer = new GridLayerImpl();

            if (gridLayerXml.Root == null) return layer;
            var gridNames = NinXmlParser.GetChildElements(gridLayerXml.Root, "navn", TillatAntall.AkkuratEn);
            layer.Name = gridNames[0].Value;

            var gridDescriptions = NinXmlParser.GetChildElements(gridLayerXml.Root, "beskrivelse", TillatAntall.MaksimaltEn);
            if (gridDescriptions.Count == 1)
                layer.Description = gridDescriptions[0].Value;

            var gridCodes = NinXmlParser.GetChildElements(gridLayerXml.Root, "kode", TillatAntall.AkkuratEn);
            layer.Code = new Code(NinXmlParser.ParseCode(gridCodes[0]));

            var gridOwners = NinXmlParser.GetChildElements(gridLayerXml.Root, "eier", TillatAntall.AkkuratEn);
            layer.Owner = new Contact(NinXmlParser.ParseContact(gridOwners[0]));

            var gridEstablishDates = NinXmlParser.GetChildElements(gridLayerXml.Root, "etablertDato", TillatAntall.AkkuratEn);
            layer.Established = Convert.ToDateTime(gridEstablishDates[0].Value);

            var gridDocuments = NinXmlParser.GetChildElements(gridLayerXml.Root, "dokumenter", TillatAntall.NullEllerFlere);
            foreach (var gridDocument in gridDocuments)
                layer.Documents.Add(new Document(NinXmlParser.ParseDocument(gridDocument)));

            var localName = gridLayerXml.Root.Name.LocalName;
            switch (localName)
            {
                case "RuteNettKart":
                    layer.RutenettkartFraXml(gridLayerXml);
                    break;
                case "OmraadeKart":
                    layer.OmrådekartFraXml(gridLayerXml);
                    break;
                default:
                    throw new GridParseException("The element " + localName +
                                                 " contains a unknown value.");
            }
            return layer;
        }

        private void OmrådekartFraXml(XDocument gridLayerXml)
        {
            var mapElements = NinXmlParser.GetChildElements(gridLayerXml.Root, "kartElementer",
                TillatAntall.EnEllerFlere);
            foreach (var mapElement in mapElements)
            {
                var gridMapElement = NinXmlParser.GetChildElements(mapElement, "OmraadeKartElement",
                    TillatAntall.AkkuratEn);

                var gridLayerCell = new GridLayerCellCustom();
                var gridLayerItemIds = NinXmlParser.GetChildElements(gridMapElement[0], "id",
                    TillatAntall.AkkuratEn);
                gridLayerCell.CellId = gridLayerItemIds[0].Value;

                var gridLayerCellGeometries = NinXmlParser.GetChildElements(gridMapElement[0], "geometri",
                    TillatAntall.AkkuratEn);
                int epsgCode;
                var geometry = NinXmlParser.ParseGeometry(gridLayerCellGeometries[0], out epsgCode);
                gridLayerCell.CustomCell = SqlGeometry.STGeomFromText(new SqlChars(geometry), epsgCode);

                var gridLayerItemValues = NinXmlParser.GetChildElements(gridMapElement[0], "trinnVerdi",
                    TillatAntall.AkkuratEn);
                gridLayerCell.Value = gridLayerItemValues[0].Value;

                if (Cells.Count == 0)
                {
                    MinValue = gridLayerCell.Value;
                    MaxValue = gridLayerCell.Value;
                }
                else
                {
                    string minValue;
                    string maxValue;
                    AreaLayerImpl.GuessMinAndMaxValues(true, gridLayerCell.Value, MinValue,
                        MaxValue, out minValue, out maxValue);
                    MinValue = minValue;
                    MaxValue = maxValue;
                }

                Cells.Add(gridLayerCell);
            }
        }

        private void RutenettkartFraXml(XDocument gridLayerXml)
        {
            bool numberCompare = true;
            Collection<XElement> gridNets = NinXmlParser.GetChildElements(gridLayerXml.Root, "ruteNett",
                TillatAntall.AkkuratEn);

            Type = TilGridType(gridNets[0].Value);

            var mapElements = NinXmlParser.GetChildElements(gridLayerXml.Root, "kartElementer",
                TillatAntall.EnEllerFlere);
            foreach (var mapElement in mapElements)
            {
                var gridMapElement = NinXmlParser.GetChildElements(mapElement, "RuteNettKartElement",
                    TillatAntall.AkkuratEn);

                var layer = new GridLayerCell();
                var gridLayerCellIds = NinXmlParser.GetChildElements(gridMapElement[0], "id", TillatAntall.AkkuratEn);
                layer.CellId = gridLayerCellIds[0].Value;

                var gridLayerCellValues = NinXmlParser.GetChildElements(gridMapElement[0], "trinnVerdi",
                    TillatAntall.AkkuratEn);
                layer.Value = gridLayerCellValues[0].Value;

                if (Cells.Count == 0)
                {
                    MinValue = layer.Value;
                    MaxValue = layer.Value;
                }
                else
                {
                    string minValue;
                    string maxValue;
                    numberCompare = AreaLayerImpl.GuessMinAndMaxValues(numberCompare, layer.Value, MinValue,
                        MaxValue, out minValue, out maxValue);
                    MinValue = minValue;
                    MaxValue = maxValue;
                }

                Cells.Add(layer);
            }
        }

        private static RutenettType TilGridType(string gridnummer)
        {
            return (RutenettType)Int32.Parse(gridnummer, CultureInfo.InvariantCulture);
        }

        public static GridLayer FromXml(XDocument gridXml)
        {
            var gridLayer = (GridLayer) ParseXml(gridXml);
            if (gridXml.Root.Name.LocalName.Equals("OmraadeKart"))
                foreach (var gridLayerCell in gridLayer.Cells)
                    MapProjection.ConvertGeometry((GridLayerCellCustom) gridLayerCell);
            return gridLayer;
        }
    }
}