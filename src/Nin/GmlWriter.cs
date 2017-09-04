using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nin.Configuration;
using Nin.Dataleveranser;
using Nin.IO.Xml;
using Nin.Types.MsSql;

namespace Nin
{
    public class GmlWriter
    {
        private readonly XmlSkriver xmlSkriver;
        private readonly XNamespace gmlNs;
        private readonly XNamespace ninNs;
        private readonly XNamespace wfsNs;
        private readonly XNamespace xsiNs;

        public GmlWriter()
        {
            ninNs = Config.Settings.Namespace.Nin;
            gmlNs = Config.Settings.Namespace.Gml;
            wfsNs = Config.Settings.Namespace.Wfs;
            xsiNs = Config.Settings.Namespace.Xsi;
            xmlSkriver = new XmlSkriver();
        }

        public XDocument ConvertToGml(IEnumerable<NatureArea> natureAreas)
        {
            var xDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"));

            var featureCollectionElement = new XElement(
                wfsNs + "FeatureCollection",
                new XAttribute(XNamespace.Xmlns + "gml", gmlNs),
                new XAttribute(XNamespace.Xmlns + "wfs", wfsNs),
                new XAttribute(XNamespace.Xmlns + "nin", ninNs),
                new XAttribute(XNamespace.Xmlns + "xsi", xsiNs),
                new XAttribute(xsiNs + "schemaLocation", ninNs.ToString() + "/NiNCoreGmleksport.xsd"),
                AddFeatureMemberElements(natureAreas)
            );

            xDocument.Add(featureCollectionElement);

            return xDocument;
        }

        private IEnumerable<XElement> AddFeatureMemberElements(IEnumerable<NatureArea> natureAreas)
        {
            return natureAreas.Select(n => new XElement(gmlNs + "featureMember",
                new NinXElement("NaturOmraade", "",
                    new XAttribute(gmlNs + "id", "NATUREAREA_" + n.UniqueId.LocalId),
                    XmlSkriver.IdentificationElement("unikId", n.UniqueId),
                    new NinXElement("versjon", "n.Version", n.Version),
                    XmlSkriver.NatureLevelElement("nivaa", n.Nivå),
                    XmlSkriver.AreaElement("omraade", n.Area, n.UniqueId.LocalId),
                    XmlSkriver.ContactElement("kartlegger", n.Surveyer),
                    new NinXElement("kartlagtDato", "n.Surveyed", n.Surveyed),
                    new NinXElement("beskrivelse", "n.Description", n.Description),
                    XmlSkriver.DocumentElements(n.Documents),
                    xmlSkriver.AddParameterElements(n.Parameters))));
        }
    }
}