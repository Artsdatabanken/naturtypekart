using System.IO;
using Nin.IO.Xml;
using NUnit.Framework;

namespace Test.Integration.Nin.Common
{
    public class GenerateDataDeliveryTest
    {
        [Test][Ignore("This unit test can only be used to generate testdata.")]
        public void GenerateDataDeliveryXmlTest()
        {
            const string kommune = "MGK";
            const int count = 2500;
            int offset = 0;
            while(true)
            {
                var dataDelivery = TestDataDelivery.Create(@"Data\Area\ar5\" + kommune + "_ar5_nin.shp", 25833, offset, count);
                if (dataDelivery == null) break;
                var dataDeliveryXml = new XmlConverter().ToXml(dataDelivery);
                File.WriteAllText($"{dataDelivery.Name}_{offset}_{count}.xml", dataDeliveryXml.ToString());
                offset += count;
            }
            //File.WriteAllText(kommune + "Ar5Test-" + rangeString + ".xml", dataDeliveryXml.ToString());
        }
    }
}
