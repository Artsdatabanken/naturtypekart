using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SqlServer.Types;
using Nin.Configuration;
using Nin.Dataleveranser;
using Nin.Types.MsSql;
using Types;

namespace Nin.IO.Xml
{
    public class XmlSkriver
    {
        public IEnumerable<XElement> NatureAreaElements(IEnumerable<NatureArea> natureAreas)
        {
            return natureAreas.Select(Naturområde);
        }

        private NinXElement Naturområde(NatureArea n)
        {
            return new NinXElement("naturOmraader", "",
                new NinXElement("NaturOmraade", "",
                    new XAttribute((XNamespace) Config.Settings.Namespace.Gml + "id",
                        "NATUREAREA_" + n.UniqueId.LocalId),
                    IdentificationElement("unikId", n.UniqueId),
                    new NinXElement("versjon", "n.Version", n.Version),
                    NatureLevelElement("nivaa", n.Nivå),
                    AreaElement("omraade", n.Area, n.UniqueId.LocalId),
                    ContactElement("kartlegger", n.Surveyer),
                    new NinXElement("kartlagtDato", "n.Surveyed", n.Surveyed),
                    new NinXElement("beskrivelse", "n.Description", n.Description),
                    DocumentElements(n.Documents),
                    AddParameterElements(n.Parameters)
                )
            );
        }

        public static XElement IdentificationElement(XName elementName, Identification identification)
        {
            var e = new NinXElement(elementName);
            if (identification == null) return e;

            var ie = new NinXElement("Identifikasjon");
            ie.Add(new NinXElement("lokalId", "identification.LocalId", identification.LocalId));
            ie.Add(new NinXElement("navnerom", "identification.NameSpace", identification.NameSpace));
            ie.Add(new NinXElement("versjonId", "identification.VersionId", identification.VersionId));
            e.Add(ie);

            return e;
        }

        public static IEnumerable<XElement> DocumentElements(IEnumerable<Document> documents)
        {
            return documents.Select(
                d => new NinXElement("dokumenter",
                    new NinXElement("Dokument", "",
                        new NinXElement("tittel", "d.Title", d.Title),
                        new NinXElement("beskrivelse", "d.Description", d.Description),
                        ContactElement("forfatter", d.Author),
                        new NinXElement("fil", "d.FileName", d.FileName)
                    )
                )
            );
        }

        public static XElement AreaElement(XName elementName, SqlGeometry area, Guid localId)
        {
            var e = new NinXElement(elementName);
            if (area == null) return e;

            var gmlXml = area.AsGml().Value;
            gmlXml = gmlXml.Replace(" xmlns=\"http://www.opengis.net/gml\"", "");

            var gmlElement = XElement.Parse(gmlXml);

            gmlElement.Add(new XAttribute((XNamespace) Config.Settings.Namespace.Gml + "id", "GEOMETRY_" + localId));
            gmlElement.Add(new XAttribute("srsName", "EPSG:" + area.STSrid));

            foreach (var gmlElementPart in gmlElement.DescendantsAndSelf())
                gmlElementPart.Name = (XNamespace) Config.Settings.Namespace.Gml + gmlElementPart.Name.LocalName;

            e.Add(gmlElement);

            return e;
        }

        public static XElement ContactElement(XName elementName, Contact contact)
        {
            var e = new NinXElement(elementName);
            if (contact == null) return e;

            var ci = new NinXElement("Kontaktinformasjon");
            ci.Add(new NinXElement("firmaNavn", "contact.Company", contact.Company));
            ci.Add(new NinXElement("kontaktPerson", "contact.ContactPerson", contact.ContactPerson));
            ci.Add(new NinXElement("email", "contact.Email", contact.Email));
            ci.Add(new NinXElement("telefon", "contact.Phone", contact.Phone));
            ci.Add(new NinXElement("hjemmeside", "contact.Homesite", contact.Homesite));
            e.Add(ci);

            return e;
        }

        public static XElement NatureLevelElement(XName elementName, NatureLevel natureLevel)
        {
            var natureLevelText = MapNatureLevelToInt(natureLevel);
            return new NinXElement(elementName, "natureLevel", natureLevelText);
        }

        private static string MapNatureLevelToInt(NatureLevel natureLevel)
        {
            return ((int) natureLevel).ToString();
        }

        public IEnumerable<XElement> AddParameterElements(List<Parameter> parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            return parameters.Select(
                p => new NinXElement("parametre", ParameterElement(p))
            );
        }

        private XElement ParameterElement(Parameter parameter)
        {
            XElement e = null;
            if (parameter.GetType() == typeof(DescriptionVariable))
                e = DescriptionVariableElement("Beskrivelsesvariabel",
                    (DescriptionVariable) parameter);
            else if (parameter.GetType() == typeof(NatureAreaType))
                e = NatureAreaTypeElement("NaturomraadeType", (NatureAreaType) parameter);

            return e;
        }

        private static XElement DescriptionVariableElement(XName elementName, DescriptionVariable descriptionVariable)
        {
            var e = new NinXElement(elementName);
            if (descriptionVariable == null) return e;

            e.Add(new NinXElement("kode", "descriptionVariable.Code", descriptionVariable.Code));
            e.Add(ContactElement("kartlegger", descriptionVariable.Surveyer));
            e.Add(new NinXElement("kartlagtDato", "descriptionVariable.Surveyed", descriptionVariable.Surveyed));
            e.Add(new NinXElement("verdi", "descriptionVariable.Value", descriptionVariable.Value));
            e.Add(new NinXElement("beskrivelse", "descriptionVariable.Description", descriptionVariable.Description));

            return e;
        }

        private XElement NatureAreaTypeElement(XName elementName, NatureAreaType natureAreaType)
        {
            var e = new NinXElement(elementName);
            if (natureAreaType == null) return e;

            e.Add(new NinXElement("kode", "natureAreaType.Code", natureAreaType.Code));
            e.Add(ContactElement("kartlegger", natureAreaType.Surveyer));
            e.Add(new NinXElement("kartlagtDato", "natureAreaType.Surveyed", natureAreaType.Surveyed));
            e.Add(new NinXElement("andel", "natureAreaType.Share", natureAreaType.Share));
            e.Add(CustomVariableElements(natureAreaType.CustomVariables));
            e.Add(AdditionalVariableElements(natureAreaType.AdditionalVariables));

            return e;
        }

        private IEnumerable<XElement> AdditionalVariableElements(IEnumerable<DescriptionVariable> additionalVariables)
        {
            return additionalVariables.Select(
                a => new NinXElement("tilleggsVariabler", ParameterElement(a))
            );
        }

        private static IEnumerable<XElement> CustomVariableElements(IEnumerable<CustomVariable> customVariables)
        {
            return customVariables.Select(
                c => new NinXElement("egendefinerteVariabler", CustomVariableElement(c, "EgendefinertVariabel"))
            );
        }

        private static XElement CustomVariableElement(CustomVariable customVariable, XName elementName)
        {
            var e = new NinXElement(elementName);
            if (customVariable == null) return e;

            e.Add(new NinXElement("betegnelse", "customVariable.Specification", customVariable.Specification));
            e.Add(new NinXElement("verdi", "customVariable.Value", customVariable.Value));

            return e;
        }
    }
}