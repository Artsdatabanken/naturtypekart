using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Nin.Common;
using Nin.Common.Map.Geometric.Grids;
using Nin.Dataleveranser;
using Nin.Rutenett;
using Nin.Types.GridTypes;
using Nin.Types.MsSql;

namespace Nin.Områder
{
    public class AreaLayerImpl : AreaLayer
    {
        public static AreaLayer FromXml(XDocument areaLayerXml)
        {
            var areaLayer = new AreaLayerImpl();
            bool numberCompare = true;

            var areaNames = NinXmlParser.GetChildElements(areaLayerXml.Root, "navn", TillatAntall.AkkuratEn);
            areaLayer.Name = areaNames[0].Value;

            var areaDescriptions = NinXmlParser.GetChildElements(areaLayerXml.Root, "beskrivelse", TillatAntall.MaksimaltEn);
            if (areaDescriptions.Count == 1)
                areaLayer.Description = areaDescriptions[0].Value;

            var areaCodes = NinXmlParser.GetChildElements(areaLayerXml.Root, "kode", TillatAntall.AkkuratEn);
            areaLayer.Code = new Code(NinXmlParser.ParseCode(areaCodes[0]));

            var areaOwners = NinXmlParser.GetChildElements(areaLayerXml.Root, "eier", TillatAntall.AkkuratEn);
            areaLayer.Owner = new Contact(NinXmlParser.ParseContact(areaOwners[0]));

            var areaEstablishDates = NinXmlParser.GetChildElements(areaLayerXml.Root, "etablertDato", TillatAntall.AkkuratEn);
            areaLayer.Established = Convert.ToDateTime(areaEstablishDates[0].Value);

            var areaDocuments = NinXmlParser.GetChildElements(areaLayerXml.Root, "dokumenter", TillatAntall.NullEllerFlere);
            foreach (var areaDocument in areaDocuments)
                areaLayer.Documents.Add(new Document(NinXmlParser.ParseDocument(areaDocument)));

            var areaAdministrationAreas = NinXmlParser.GetChildElements(areaLayerXml.Root, "administrativtOmraade", TillatAntall.AkkuratEn);
            switch (areaAdministrationAreas[0].Value)
            {
                case "1":
                    areaLayer.Type = AreaType.Kommune;
                    break;
                case "2":
                    areaLayer.Type = AreaType.Fylke;
                    break;
                default:
                    throw new GridParseException("The element " + areaAdministrationAreas[0].Name.LocalName +
                                                 " contains a unknown value.");
            }

            var mapElements = NinXmlParser.GetChildElements(areaLayerXml.Root, "kartElementer", TillatAntall.EnEllerFlere);
            foreach (var mapElement in mapElements)
            {
                var areaMapElement = NinXmlParser.GetChildElements(mapElement, "AdministrativtOmraadeKartElement", TillatAntall.AkkuratEn);

                var areaLayerItem = new AreaLayerItem();
                var areaLayerItemNumbers = NinXmlParser.GetChildElements(areaMapElement[0], "nr", TillatAntall.AkkuratEn);
                areaLayerItem.Number = int.Parse(areaLayerItemNumbers[0].Value);

                var areaLayerItemValues = NinXmlParser.GetChildElements(areaMapElement[0], "trinnVerdi", TillatAntall.AkkuratEn);
                areaLayerItem.Value = areaLayerItemValues[0].Value;

                if (areaLayer.Items.Count == 0)
                {
                    areaLayer.MinValue = areaLayerItem.Value;
                    areaLayer.MaxValue = areaLayerItem.Value;
                }
                else
                {
                    string minValue;
                    string maxValue;
                    numberCompare = GuessMinAndMaxValues(numberCompare, areaLayerItem.Value, areaLayer.MinValue, areaLayer.MaxValue, out minValue, out maxValue);
                    areaLayer.MinValue = minValue;
                    areaLayer.MaxValue = maxValue;
                }

                areaLayer.Items.Add(areaLayerItem);
            }

            return areaLayer;
        }

        public static bool GuessMinAndMaxValues(bool tryNumericCompare, string value, string minValueIn, string maxValueIn, out string minValueOut, out string maxValueOut)
        {
            bool numberCompare = tryNumericCompare;
            minValueOut = minValueIn;
            maxValueOut = maxValueIn;

            if (tryNumericCompare)
            {
                double minValue;
                double maxValue = 0.0;
                double v = 0.0;
                numberCompare = double.TryParse(minValueIn, NumberStyles.Any, CultureInfo.InvariantCulture, out minValue);

                if (numberCompare)
                    numberCompare = double.TryParse(maxValueIn, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out maxValue);

                if (numberCompare)
                    numberCompare = double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out v);

                if (numberCompare)
                {
                    if (v > maxValue)
                        maxValueOut = "" + value;
                    if (v < minValue)
                        minValueOut = "" + value;
                }
            }

            if (numberCompare) return true;
            if (string.Compare(value, maxValueIn, StringComparison.InvariantCulture) > 0)
                maxValueOut = value;
            if (string.Compare(value, minValueIn, StringComparison.InvariantCulture) < 0)
                minValueOut = value;

            return false;
        }

        public static Layer FraAdministrativtOmrådeNinXml(string dataFile)
        {
            XDocument xml = XDocument.Load(File.OpenRead(dataFile));
            if (xml.Root.Name.LocalName == "AdministrativtOmraadeKart")
                return FromXml(xml);
            return GridLayerImpl.FromXml(xml);
        }
    }
}