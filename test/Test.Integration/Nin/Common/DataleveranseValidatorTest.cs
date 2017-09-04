using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Nin;
using Nin.Common;
using Nin.Configuration;
using Nin.Dataleveranser;
using Nin.Test.Integration.NiN.Common;
using NUnit.Framework;

namespace Test.Integration.Nin.Common
{
    public class DataleveranseValidatorTest
    {
        private readonly DataleveranseValiderer dataDeliveryImportValidator;
        private readonly DataleveranseValiderer dataDeliveryExportValidator;
        private readonly DataleveranseValiderer dataDeliveryMapImportValidator;

        public DataleveranseValidatorTest()
        {
            //var findDirectoryInTree = TestSetup.FindDirectoryInTree(@"schema\cache\");
            var xsd = FileLocator.FindFileInTree(@"schema\cache\ogc_schema_updates.rss");
            var xsdPath = Path.GetDirectoryName(xsd);

            GmlXmlResolver gmlXmlResolver = DataleveranseXmlGreier.CreateGmlXmlResolver(xsdPath);

            dataDeliveryImportValidator = CreateValidator(gmlXmlResolver, @"schema\NiNCoreDataLeveranse.xsd");
            dataDeliveryExportValidator = CreateValidator(gmlXmlResolver, @"schema\NiNCoreDataEksport.xsd");
            dataDeliveryMapImportValidator = CreateValidator(gmlXmlResolver, @"schema\NiNCoreGridLeveranse.xsd");
        }

        private static DataleveranseValiderer CreateValidator(XmlResolver gmlXmlResolver, string xsdPath)
        {
            var schemas = new XmlSchemaSet {XmlResolver = gmlXmlResolver};

            using (StreamReader xsdMarkup = File.OpenText(FileLocator.FindFileInTree(xsdPath)))
            {
                var xmlReader = XmlReader.Create(xsdMarkup);
                schemas.Add("http://pavlov.itea.ntnu.no/NbicFiles", xmlReader);
            }

            return new DataleveranseValiderer(schemas);
        }

        [Test]
        public void ValidateDataDeliveryTest_ValidationOk()
        {
            XDocument dataDeliveryXml = TestXml.ReadXDocument(@"data\NatureArea\NiNCoreImportExample.xml");
            dataDeliveryImportValidator.ValidateDataDelivery(dataDeliveryXml);
        }

        [Test]
        public void ValidateDataDeliveryExportTest_ValidationOk()
        {
            XDocument dataDeliveryXml = TestXml.ReadXDocument(@"Data\NatureArea\NiNCoreExportExample.xml");
            dataDeliveryExportValidator.ValidateDataDelivery(dataDeliveryXml);
        }

        [Test]
        public void ValidateAreaMapImport_ValidationOk()
        {
            XDocument areaMapXml = TestXml.ReadXDocument(@"Data\AreaLayer\OmraadeKartTest.xml");
            dataDeliveryMapImportValidator.ValidateDataDelivery(areaMapXml);
        }

        [Test]
        public void ValidateGridMapImport_ValidationOk()
        {
            var gridMapXml = TestXml.ReadXDocument(@"Data\GridLayer\RuteNettKartTest.xml");
            dataDeliveryMapImportValidator.ValidateDataDelivery(gridMapXml);
        }

        [Test]
        public void ValidateAdminAreaMapImport_ValidationOk()
        {
            var areaMapXml = TestXml.ReadXDocument(@"Data\AreaLayer\AdministrativtOmraadeKartTest.xml");
            dataDeliveryMapImportValidator.ValidateDataDelivery(areaMapXml);
        }
    }
}
