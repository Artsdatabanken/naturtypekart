using System.IO;
using System.Text;
using System.Xml.Linq;
using Nin.Configuration;

namespace Nin.Test.Integration.NiN.Common
{
    public static class TestXml {
        public static XDocument ReadXDocument(string xmlFilePath)
        {
            var path = FileLocator.FindFileInTree(xmlFilePath);
            string gridMapXmlText = File.ReadAllText(path, Encoding.GetEncoding("iso-8859-1"));
            XDocument gridMapXml = XDocument.Parse(gridMapXmlText);
            return gridMapXml;
        }
    }
}