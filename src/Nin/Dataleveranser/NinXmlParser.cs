using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DotSpatial.Projections;
using Microsoft.SqlServer.Types;
using Nin.Common;
using Nin.Types.RavenDb;

namespace Nin.Dataleveranser
{
    public static class NinXmlParser
    {
        public static Contact ParseContact(XElement xElement)
        {
            var contact = new Contact();

            if (xElement == null) return contact;
            var contactElement = GetChildElements(xElement, "Kontaktinformasjon", TillatAntall.AkkuratEn);

            var ce = contactElement[0];
            var contactCompanies = GetChildElements(ce, "firmaNavn", TillatAntall.MaksimaltEn);
            if (contactCompanies.Count == 1)
                contact.Company = contactCompanies[0].Value;

            var contactPersons = GetChildElements(ce, "kontaktPerson", TillatAntall.MaksimaltEn);
            if (contactPersons.Count == 1)
                contact.ContactPerson = contactPersons[0].Value;

            var contactEmails = GetChildElements(ce, "email", TillatAntall.MaksimaltEn);
            if (contactEmails.Count == 1)
                contact.Email = contactEmails[0].Value;

            var contactPhones = GetChildElements(ce, "telefon", TillatAntall.MaksimaltEn);
            if (contactPhones.Count == 1)
                contact.Phone = contactPhones[0].Value;

            var contactHomesites = GetChildElements(ce, "hjemmeside", TillatAntall.MaksimaltEn);
            if (contactHomesites.Count == 1)
                contact.Homesite = contactHomesites[0].Value;

            return contact;
        }

        public static Code ParseCode(XElement xElement)
        {
            var code = new Code();
            if (xElement == null) return code;

            var codeElement = GetChildElements(xElement, "Kode", TillatAntall.AkkuratEn);

            var first = codeElement[0];
            var codeRegistries = GetChildElements(first, "koderegister", TillatAntall.AkkuratEn);
            code.Registry = codeRegistries[0].Value;

            var codeVersions = GetChildElements(first, "kodeversjon", TillatAntall.AkkuratEn);
            code.Version = codeVersions[0].Value;

            var codeValues = GetChildElements(first, "kode", TillatAntall.AkkuratEn);
            code.Value = codeValues[0].Value;

            return code;
        }

        public static Document ParseDocument(XElement xElement)
        {
            var document = new Document();
            if (xElement == null) return document;

            var documentElement = GetChildElements(xElement, "Dokument", TillatAntall.AkkuratEn);

            var element = documentElement[0];
            var documentTitles = GetChildElements(element, "tittel", TillatAntall.AkkuratEn);
            document.Title = documentTitles[0].Value;

            var documentDescriptions = GetChildElements(element, "beskrivelse", TillatAntall.MaksimaltEn);
            if (documentDescriptions.Count == 1)
                document.Description = documentDescriptions[0].Value;

            var documentAuthors = GetChildElements(element, "forfatter", TillatAntall.MaksimaltEn);
            if (documentAuthors.Count == 1)
                document.Author = ParseContact(documentAuthors[0]);

            var documentFiles = GetChildElements(element, "fil", TillatAntall.AkkuratEn);
            document.FileName = documentFiles[0].Value;

            return document;
        }

        public static Collection<XElement> GetChildElements(XElement xElement, string xElementLocalName, TillatAntall expected)
        {
            var elements = new Collection<XElement>(
                (
                    from element in xElement.Elements() where element.Name.LocalName == xElementLocalName select element
                ).ToList()
            );

            string toManyElementsMessage = "The '" + xElement.Name.LocalName + "' element contains to many '" + xElementLocalName + "' elements";
            string missingElementMessage = "The '" + xElement.Name.LocalName + "' element is missing the mandatory element '" + xElementLocalName + "'";

            switch (expected)
            {
                case TillatAntall.MaksimaltEn:
                    if (elements.Count > 1)
                        throw new CommonParseException(toManyElementsMessage);
                    break;
                case TillatAntall.AkkuratEn:
                    if (elements.Count == 0)
                        throw new CommonParseException(missingElementMessage);
                    if (elements.Count > 1)
                        throw new CommonParseException(toManyElementsMessage);
                    break;
                case TillatAntall.EnEllerFlere:
                    if (elements.Count == 0)
                        throw new CommonParseException(missingElementMessage);
                    break;
            }

            return elements;
        }

        public static string ParseGeometry(XElement xElement, out int epsgCode)
        {
            // TODO: Make this function fully functional due to the GML 3.2 standard
            string geometry = "";

            epsgCode = 0;

            if (xElement == null) return geometry;
            var geometryElements = new Collection<XElement>(xElement.Elements().ToList());

            if (geometryElements.Count == 1)
            {
                var geometryElementAttributes = GetAttributes(geometryElements[0]);

                // TODO: Temporary espg code decoding
                if (!geometryElementAttributes.ContainsKey("srsName"))
                    throw new CommonParseException("The '" + geometryElements[0].Name.LocalName +
                                                   "' element does not contain the 'srsName' attribute");
                var srsName = geometryElementAttributes["srsName"].Replace("\"", "");
                var srsNameParts = srsName.Split(':');
                epsgCode = int.Parse(srsNameParts[srsNameParts.Length - 1]);

                if (geometryElementAttributes.ContainsKey("srsDimension"))
                {
                    var srsDimension = int.Parse(geometryElementAttributes["srsDimension"].Replace("\"", ""));
                    if (srsDimension > 2)
                    {
                        XNamespace gmlNamespace = "http://www.opengis.net/gml/3.2";
                        XName posListName = gmlNamespace + "posList";
                        XName posName = gmlNamespace + "pos";

                        foreach (var geoElement in geometryElements[0].Descendants())
                        {
                            if (geoElement.Name != posListName && geoElement.Name != posName) continue;

                            var xyPosList = "";
                            var coordinates = geoElement.Value.Split(' ');

                            int coordinateNumber = 1;
                            foreach (var coordinate in coordinates)
                            {
                                if (coordinateNumber < 3)
                                {
                                    xyPosList += coordinate + " ";
                                    coordinateNumber++;
                                }
                                else if (coordinateNumber == srsDimension)
                                    coordinateNumber = 1;
                                else
                                    coordinateNumber++;
                            }
                            geoElement.Value = xyPosList;
                        }
                    }
                }

                geometryElements[0].RemoveAttributes();
                string xmlData = geometryElements[0].ToString();

                // Microsoft.SqlServer.Types does not support gml v3.2:
                xmlData = xmlData.Replace("http://www.opengis.net/gml/3.2", "http://www.opengis.net/gml");

                var xmlTextReader = new XmlTextReader(xmlData, XmlNodeType.Document, null);
                var sqlXml = new SqlXml(xmlTextReader);

                if (IsLatitudeLongitude(epsgCode))
                {
                    var sqlGeography = SqlGeography.GeomFromGml(sqlXml, epsgCode);
                    geometry = sqlGeography.ToString();
                }
                else
                {
                    var sqlGeometry = SqlGeometry.GeomFromGml(sqlXml, epsgCode);
                    geometry = sqlGeometry.ToString();
                }
            }
            else if (geometryElements.Count > 1)
                throw new CommonParseException("The '" + xElement.Name.LocalName +
                                               "' element contains to many geometry elements");
            return geometry;
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

        private static bool IsLatitudeLongitude(int epsgCode)
        {
            try
            {
                return ProjectionInfo.FromEpsgCode(epsgCode).IsLatLon;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new DataDeliveryParseException("Unknown EPSG code: " + epsgCode);
            }
        }
    }
}
