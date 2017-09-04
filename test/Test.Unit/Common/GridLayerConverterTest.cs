using System;
using Nin.Dataleveranser.Rutenett;
using Nin.IO.Xml;
using Nin.Rutenett;
using Nin.Types.MsSql;
using NUnit.Framework;

namespace Test.Unit.Common
{
    class GridLayerConverterTest
    {
        [Test][Ignore("TODO: Mangler fil")]
        public void ConvertTemperatureGridLayerTest()
        {
            const string excelFilePath = @"C:\Artsdatabanken\NIBIO\NIBO_SSB_5x5_export.xlsx";
            var gridLayer = GridLayerImpl.FromExcelFile(excelFilePath, RutenettType.SSB005KM, "Temperatur Test", 6);

            gridLayer.Established = new DateTime(2016, 3, 7);
            gridLayer.Code = new Code
            {
                Value = "TEMP",
                Registry = "NiN",
                Version = "2.0"
            };
            gridLayer.Owner = new Contact
            {
                Company = "NIBIO",
                Homesite = "www.nibio.no"
            };

            var gridXml = new XmlConverter().ToXml(gridLayer);
            gridXml.Save(@"C:\Artsdatabanken\KartTestData\RuteNettKartTemperatur.xml");
        }

        [Test][Ignore("Fil mangler. Kan du sjekke inn fila?")]
        public void ConvertGridLayerTest()
        {
            const string excelFilePath = @"C:\Artsdatabanken\NIBIO\SSB1000test.xlsx";
            var gridLayer = GridLayerImpl.FromExcelFile(excelFilePath, RutenettType.SSB001KM, "Antall landbruk Test", 2);

            gridLayer.Established = new DateTime(2016, 3, 7);
            gridLayer.Code = new Code
            {
                Value = "LB",
                Registry = "NiN",
                Version = "2.0"
            };
            gridLayer.Owner = new Contact
            {
                Company = "NIBIO",
                Homesite = "www.nibio.no"
            };

            var gridXml = new XmlConverter().ToXml(gridLayer);
            gridXml.Save(@"C:\Artsdatabanken\KartTestData\RuteNettKartLandbruk.xml");
        }
    }
}
