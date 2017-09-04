using System.Xml.Linq;
using Nin.Områder;
using Nin.Rutenett;
using Nin.Test.Integration.NiN.Common;
using NUnit.Framework;
using GridLayer = Nin.Types.GridTypes.GridLayer;

namespace Test.Integration.Nin.Common
{
    public class GridParserTest
    {
        [Test]
        public void ParseAreaMapTest_ParseOk()
        {
            XDocument areaLayerXml = TestXml.ReadXDocument(@"Data\AreaLayer\OmraadeKartTest.xml");

            GridLayer areaLayer = GridLayerImpl.FromXml(areaLayerXml);
            Assert.NotNull(areaLayer);
        }

        [Test]
        public void ParseGridMapTest_ParseOk()
        {
            XDocument gridLayerXml = TestXml.ReadXDocument(@"Data\GridLayer\RuteNettKartTest.xml");

            GridLayer gridLayer = GridLayerImpl.FromXml(gridLayerXml);

            Assert.NotNull(gridLayer);
        }

        [Test]
        public void ParseAdminAreaMapTest_ParseOk()
        {
            XDocument areaLayerXml = TestXml.ReadXDocument(@"Data\AreaLayer\AdministrativtOmraadeKartTest.xml");

            var areaLayer = AreaLayerImpl.FromXml(areaLayerXml);

            Assert.NotNull(areaLayer);
        }
    }
}
