using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Nin.Configuration;
using Nin.Dataleveranser;
using Nin.Types;
using Nin.Types.GridTypes;
using Nin.Types.MsSql;
using Code = Nin.Types.MsSql.Code;
using CustomVariableDefinition = Nin.Types.MsSql.CustomVariableDefinition;
using NinStandardVariabel = Nin.Types.MsSql.NinStandardVariabel;

namespace Nin.IO.Xml
{
    public class XmlConverter
    {
        private readonly XmlSkriver xmlSkriver;
        private readonly XNamespace gmlNs;

        public XmlConverter()
        {
            xmlSkriver = new XmlSkriver();
            gmlNs = Config.Settings.Namespace.Gml;
        }

        public XDocument ToXml(IEnumerable<Metadata> metadatas)
        {
            var xDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"));

            XElement dataExportElement = new NinXElement("DataEksport", "",
                new XAttribute(XNamespace.Xmlns + "gml", gmlNs),
                new XAttribute(XNamespace.Xmlns + "nin", Config.Settings.Namespace.Nin),
                new NinXElement("eksportDato", "DateTime.Now", DateTime.Now),
                AddMetadataElements(metadatas)
            );

            xDocument.Add(dataExportElement);

            var emptyElements = from element in xDocument.Descendants() where element.IsEmpty select element;

            while (emptyElements.Any())
                emptyElements.Remove();

            return xDocument;
        }

        public XDocument ToXml(GridLayer gridLayer)
        {
            var xDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"));

            XElement gridNetElement = new NinXElement("RuteNettKart", "",
                new XAttribute(XNamespace.Xmlns + "nin", Config.Settings.Namespace.Nin),
                new XAttribute(XNamespace.Xmlns + "gml", gmlNs),
                new NinXElement("navn", "gridLayer.Name", gridLayer.Name),
                new NinXElement("beskrivelse", "gridLayer.Description", gridLayer.Description),
                new NinXElement("kode", "AddCodeElement('Kode', gridLayer.Code)",
                    AddCodeElement("Kode", gridLayer.Code)),
                XmlSkriver.ContactElement("eier", gridLayer.Owner),
                new NinXElement("etablertDato", "gridLayer.Established", gridLayer.Established),
                new NinXElement("ruteNett", "(int)gridLayer.Type", (int) gridLayer.Type),
                AddMapElements(gridLayer.Cells, gridLayer.Code != null ? gridLayer.Code.Value : string.Empty)
            );

            xDocument.Add(gridNetElement);

            var emptyElements = from element in xDocument.Descendants() where element.IsEmpty select element;

            while (emptyElements.Any())
                emptyElements.Remove();

            return xDocument;
        }

        public XDocument ToXml(Dataleveranse dataleveranse)
        {
            var xDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"));

            var dataDeliveryElement = new NinXElement("DataLeveranse", "",
                new XAttribute(XNamespace.Xmlns + "gml", gmlNs),
                new XAttribute(XNamespace.Xmlns + "nin", Config.Settings.Namespace.Nin),
                new NinXElement("navn", "dataDelivery.Name", dataleveranse.Name),
                new NinXElement("leveranseDato", "dataDelivery.DeliveryDate", dataleveranse.DeliveryDate),
                XmlSkriver.ContactElement("operatoer", dataleveranse.Operator),
                new NinXElement("grunnForEndring", "dataDelivery.ReasonForChange", dataleveranse.ReasonForChange),
                new NinXElement("beskrivelse", "dataDelivery.Description", dataleveranse.Description),
                AddMetadataElements(new Collection<Metadata> {dataleveranse.Metadata})
            );
            dataDeliveryElement.Print("");
            xDocument.Add(dataDeliveryElement);

            var emptyElements = from element in xDocument.Descendants() where element.IsEmpty select element;

            while (emptyElements.Any())
                emptyElements.Remove();

            return xDocument;
        }

        private IEnumerable<XElement> AddMetadataElements(IEnumerable<Metadata> metadatas)
        {
            return metadatas.Select(
                m => new NinXElement("metadata", "",
                    new NinXElement("Metadata", "",
                        new XAttribute(gmlNs + "id", "METADATA_" + m.UniqueId.LocalId),
                        XmlSkriver.IdentificationElement("unikId", m.UniqueId),
                        new NinXElement("program", "m.Program", m.Program),
                        new NinXElement("prosjektnavn", "m.ProjectName", m.ProjectName),
                        new NinXElement("prosjektbeskrivelse", "m.ProjectDescription", m.ProjectDescription),
                        new NinXElement("formaal", "m.Purpose", m.Purpose),
                        XmlSkriver.ContactElement("oppdragsgiver", m.Contractor),
                        XmlSkriver.ContactElement("eier", m.Owner),
                        new NinXElement("kartlagtFraDato", "m.SurveyedFrom", m.SurveyedFrom),
                        new NinXElement("kartlagtTilDato", "m.SurveyedTo", m.SurveyedTo),
                        new NinXElement("kartleggingsMaalestokk", "m.SurveyScale", m.SurveyScale),
                        new NinXElement("opploesning", "m.Resolution", m.Resolution),
                        XmlSkriver.AreaElement("dekningsOmraade", m.Area, m.UniqueId.LocalId),
                        AddQualityElement("kvalitet", m.Quality),
                        XmlSkriver.DocumentElements(m.Documents),
                        xmlSkriver.NatureAreaElements(m.NatureAreas),
                        AddVariableDefinitions(m.VariabelDefinitions)
                    )
                )
            );
        }

        private static IEnumerable<XElement> AddVariableDefinitions(
            IEnumerable<NinVariabelDefinisjon> variableDefinitions)
        {
            return variableDefinitions.Select(
                v => new NinXElement("variabelDefinisjoner", "AddVariableDefinition(v)", AddVariableDefinition(v))
            );
        }

        private static XElement AddVariableDefinition(NinVariabelDefinisjon vd)
        {
            XElement e = null;
            if (vd.GetType() == typeof(CustomVariableDefinition))
                e = AddCustomVariableDefinitionElement("EgendefinertVariabelDefinisjon",
                    (CustomVariableDefinition) vd);
            else if (vd.GetType() == typeof(NinStandardVariabel))
                e = AddStandardVariableElement("StandardisertVariabel",
                    (NinStandardVariabel) vd);

            return e;
        }

        private static XElement AddCustomVariableDefinitionElement(XName elementName,
            CustomVariableDefinition customVariableDefinition)
        {
            var e = new NinXElement(elementName);
            if (customVariableDefinition == null) return e;

            e.Add(new NinXElement("betegnelse", "customVariableDefinition.Specification)",
                customVariableDefinition.Specification));
            e.Add(new NinXElement("beskrivelse", "customVariableDefinition.Description)",
                customVariableDefinition.Description));

            return e;
        }

        private static XElement AddStandardVariableElement(XName elementName, NinStandardVariabel standardVariable)
        {
            var sv = new NinXElement(elementName);

            if (standardVariable != null)
                sv.Add(AddVariableDefinitionElement("variabelDefinisjon",
                    standardVariable.VariableDefinition));

            return sv;
        }

        private static XElement AddVariableDefinitionElement(XName elementName, Code code)
        {
            var vd = new NinXElement(elementName);
            if (code == null) return vd;

            vd.Add(AddCodeElement("Kode", code));
            return vd;
        }

        private static XElement AddCodeElement(XName elementName, Types.Code code)
        {
            var codeElement = new NinXElement(elementName);
            if (code == null) return codeElement;

            codeElement.Add(new NinXElement("koderegister", "code.Registry)", code.Registry));
            codeElement.Add(new NinXElement("kodeversjon", "code.Version)", code.Version));
            codeElement.Add(new NinXElement("kode", "code.Value)", code.Value));

            return codeElement;
        }

        private static XElement AddQualityElement(XName elementName, Quality quality)
        {
            var e = new NinXElement(elementName);

            if (quality == null) return e;
            var q = new NinXElement("Posisjonskvalitet");
            q.Add(new NinXElement("maalemetode", "quality.MeasuringMethod)",
                quality.MeasuringMethod));
            q.Add(new NinXElement("noeyaktighet", "quality.Accuracy)", quality.Accuracy));
            q.Add(new NinXElement("synbarhet", "quality.Visibility)", quality.Visibility));
            q.Add(new NinXElement("maalemetodeHoeyde", "quality.MeasuringMethodHeight)",
                quality.MeasuringMethodHeight));
            q.Add(new NinXElement("noeyaktighetHoeyde", "quality.AccuracyHeight)",
                quality.AccuracyHeight));
            q.Add(new NinXElement("maksimaltAvvik", "quality.MaxDeviation)",
                quality.MaxDeviation));
            e.Add(q);

            return e;
        }

        private IEnumerable<XElement> AddMapElements(IEnumerable<GridLayerCell> gridCells, string codeValue)
        {
            return gridCells.Select(
                glc => new NinXElement("kartElementer",
                    AddMapElement(glc, codeValue)
                )
            );
        }

        private XElement AddMapElement(GridLayerCell gridCell, string codeValue)
        {
            return new NinXElement("RuteNettKartElement", "",
                new XAttribute(gmlNs + "id", codeValue + "_" + gridCell.CellId),
                new NinXElement("id", "gridCell.CellId", gridCell.CellId),
                new NinXElement("trinnVerdi", "gridCell.Value", gridCell.Value)
            );
        }
    }
}