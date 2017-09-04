using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Nin.Dataleveranser.Import
{
    public abstract class IDataFile
    {
        public abstract string ContentType { get; }
        public abstract string Filename { get; }
        public abstract Stream OpenReadStream();

        public XDocument ReadXml()
        {
            string fileContent;
            using (var streamReader = new StreamReader(OpenReadStream(), Encoding.GetEncoding("iso-8859-1")))
                fileContent = streamReader.ReadToEnd();

            XDocument dataDeliveryXml = XDocument.Parse(fileContent);
            return dataDeliveryXml;
        }
    }
}