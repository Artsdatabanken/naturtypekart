using System.Text;
using System.Xml.Linq;
using Nin.Configuration;
using Nin.Dataleveranser;
using Nin.Types.RavenDb;
using NUnit.Framework;
using File = System.IO.File;

namespace Test.Integration.Nin.Common
{
    public class DataleveranseParserTest
    {
        [Test]
        public void ParseDataDeliveryTest_ParseOk()
        {
            string path = FileLocator.FindFileInTree(@"Data\NatureArea\NiNCoreImportExample.xml");
            string dataDeliveryXmlText = File.ReadAllText(path, Encoding.GetEncoding("iso-8859-1"));

            XDocument dataDeliveryXml = XDocument.Parse(dataDeliveryXmlText);

            Dataleveranse dataleveranse = DataleveranseParser.ParseDataleveranse(dataDeliveryXml);
            Assert.NotNull(dataleveranse);
        }
    }
}
