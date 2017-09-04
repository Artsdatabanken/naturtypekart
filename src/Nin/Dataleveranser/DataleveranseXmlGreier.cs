using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Nin.Configuration;
using Nin.Diagnostic;
using Nin.IO;
using Nin.Types.RavenDb;
using File = System.IO.File;

namespace Nin.Dataleveranser
{
    public class DataleveranseXmlGreier
    {
        public static GmlXmlResolver CreateGmlXmlResolver(string xsdCachePath)
        {
            Log.i("XSD", "Reading schemas from " + xsdCachePath);
            var resolver = new GmlXmlResolver(new Uri("file:///" + xsdCachePath + "/"));

            resolver.UriMap.Add("http://shapechange.net/resources/schema/ShapeChangeAppinfo.xsd", "ShapeChangeAppinfo.xsd");
            resolver.UriMap.Add("http://www.w3.org/1999/xlink.xsd", "1999/xlink.xsd");
            resolver.UriMap.Add("http://www.w3.org/2001/xml.xsd", "2001/xml.xsd");
            resolver.UriMap.Add("http://schemas.opengis.net/gml/3.2.1/gml.xsd", "gml/3.2.1/gml.xsd");
            resolver.UriMap.Add("http://schemas.opengis.net/iso/19139/20070417/gmd/gmd.xsd", "iso/19139/20070417/gmd/gmd.xsd");
            resolver.UriMap.Add("http://schemas.opengis.net/iso/19139/20070417/gco/gco.xsd", "iso/19139/20070417/gco/gco.xsd");
            resolver.UriMap.Add("http://schemas.opengis.net/iso/19139/20070417/gss/gss.xsd", "iso/19139/20070417/gss/gss.xsd");
            resolver.UriMap.Add("http://schemas.opengis.net/iso/19139/20070417/gts/gts.xsd", "iso/19139/20070417/gts/gts.xsd");
            resolver.UriMap.Add("http://schemas.opengis.net/iso/19139/20070417/gsr/gsr.xsd", "iso/19139/20070417/gsr/gsr.xsd");
            return resolver;
        }

        private static Dictionary<string, string> GetAttributes(XElement xElement)
        {
            var attributes = new Dictionary<string, string>();
            var attribute = xElement.FirstAttribute;
            while (attribute != null)
            {
                var attributeText = attribute.ToString();
                var attributeParts = attributeText.Split('=');
                if (attributeParts.Length == 2)
                    attributes[attributeParts[0]] = attributeParts[1];
                attribute = attribute.NextAttribute;
            }
            return attributes;
        }

        public static Dataleveranse ParseDataDelivery(XDocument dataDeliveryXml)
        {
            return DataleveranseParser.ParseDataleveranse(dataDeliveryXml);
        }

        public void ValidateDataDelivery(XDocument dataDeliveryXml)
        {
            dataleveranseValiderer.ValidateDataDelivery(dataDeliveryXml);
        }

        public void ValidateGrid(XDocument gridXml)
        {
            gridValidator.ValidateDataDelivery(gridXml);
        }

        public static void ValidateDataDeliveryContent(Dataleveranse dataleveranse)
        {
            var validator = new DataDeliveryContentValidator();
            validator.ValidateDataDeliveryContent(dataleveranse);
        }

        public DataleveranseXmlGreier()
        {
            var schemaPath = FileLocator.FindDirectoryInTree(Config.Settings.SchemaSubdirectory);
            var xsdPath = Path.Combine(schemaPath, "NiNCoreDataleveranse.xsd");
            var xsdGridPath = Path.Combine(schemaPath, "NiNCoreGridleveranse.xsd");
            var xsdCachePath = Path.Combine(schemaPath, "cache");

            var gmlXmlResolver = CreateGmlXmlResolver(xsdCachePath);

            var schemas = new XmlSchemaSet {XmlResolver = gmlXmlResolver};

            var xsdMarkup = File.ReadAllText(xsdPath);

            var xsdDocument = XDocument.Parse(xsdMarkup);
            var xsdAttributes = GetAttributes(xsdDocument.Root);
            var xsdNamespace = xsdAttributes.ContainsKey("xmlns:nin")
                ? xsdAttributes["xmlns:nin"].Replace("\"", "")
                : "";

            var stringReader = new StringReader(xsdMarkup);
            var xmlReader = XmlReader.Create(stringReader);
            schemas.Add(xsdNamespace, xmlReader);

            dataleveranseValiderer = new DataleveranseValiderer(schemas);

            schemas = new XmlSchemaSet {XmlResolver = gmlXmlResolver};

            xsdMarkup = File.ReadAllText(xsdGridPath);
            xsdDocument = XDocument.Parse(xsdMarkup);
            xsdAttributes = GetAttributes(xsdDocument.Root);
            xsdNamespace = xsdAttributes.ContainsKey("xmlns:nin") ? xsdAttributes["xmlns:nin"].Replace("\"", "") : "";

            stringReader = new StringReader(xsdMarkup);
            xmlReader = XmlReader.Create(stringReader);
            schemas.Add(xsdNamespace, xmlReader);

            gridValidator = new DataleveranseValiderer(schemas);
        }

        private readonly DataleveranseValiderer dataleveranseValiderer;
        private readonly DataleveranseValiderer gridValidator;
    }
}