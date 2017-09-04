using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Linq;
using Nin.Types.RavenDb;
using Raven.Abstractions.Extensions;
using Types;
using CustomVariable = Nin.Types.RavenDb.CustomVariable;
using CustomVariableDefinition = Nin.Types.RavenDb.CustomVariableDefinition;
using DescriptionVariable = Nin.Types.RavenDb.DescriptionVariable;
using Identification = Nin.Types.RavenDb.Identification;
using Metadata = Nin.Types.RavenDb.Metadata;
using NatureArea = Nin.Types.RavenDb.NatureArea;
using NatureAreaType = Nin.Types.RavenDb.NatureAreaType;
using Quality = Nin.Types.RavenDb.Quality;
using NinStandardVariabel = Nin.Types.RavenDb.NinStandardVariabel;

namespace Nin.Dataleveranser
{
    public static class DataleveranseParser
    {
        public static Dataleveranse ParseDataleveranse(XDocument dataDeliveryXml)
        {
            var dataDelivery = new Dataleveranse();

            if (dataDeliveryXml.Root == null) return dataDelivery;
            var dataDeliveryNames =
                NinXmlParser.GetChildElements(dataDeliveryXml.Root, "navn", TillatAntall.AkkuratEn);
            dataDelivery.Name = dataDeliveryNames[0].Value;

            var dataDeliveryDeliveryDates = NinXmlParser.GetChildElements(dataDeliveryXml.Root, "leveranseDato",
                TillatAntall.AkkuratEn);
            dataDelivery.DeliveryDate = Convert.ToDateTime(dataDeliveryDeliveryDates[0].Value);

            var dataDeliveryOperators =
                NinXmlParser.GetChildElements(dataDeliveryXml.Root, "operatoer", TillatAntall.AkkuratEn);
            dataDelivery.Operator = NinXmlParser.ParseContact(dataDeliveryOperators[0]);

            var dataDeliveryMetadatas =
                NinXmlParser.GetChildElements(dataDeliveryXml.Root, "metadata", TillatAntall.AkkuratEn);
            dataDelivery.Metadata = ParseMetadata(dataDeliveryMetadatas[0]);

            var dataDeliveryReasonForChanges = NinXmlParser.GetChildElements(dataDeliveryXml.Root, "grunnForEndring",
                TillatAntall.MaksimaltEn);
            if (dataDeliveryReasonForChanges.Count == 1)
                dataDelivery.ReasonForChange = dataDeliveryReasonForChanges[0].Value;

            var dataDeliveryDescriptions = NinXmlParser.GetChildElements(dataDeliveryXml.Root, "beskrivelse",
                TillatAntall.MaksimaltEn);
            if (dataDeliveryDescriptions.Count == 1)
                dataDelivery.Description = dataDeliveryDescriptions[0].Value;

            return dataDelivery;
        }

        private static Identification ParseIdentification(XElement xElement)
        {
            var identification = new Identification();

            if (xElement == null) return identification;
            var identificationElement =
                NinXmlParser.GetChildElements(xElement, "Identifikasjon", TillatAntall.AkkuratEn);

            var identificationLocalIds =
                NinXmlParser.GetChildElements(identificationElement[0], "lokalId", TillatAntall.AkkuratEn);
            identification.LocalId = new Guid(identificationLocalIds[0].Value);

            var identificationNamespaces =
                NinXmlParser.GetChildElements(identificationElement[0], "navnerom", TillatAntall.AkkuratEn);
            identification.NameSpace = identificationNamespaces[0].Value;

            var identificationVersionIds = NinXmlParser.GetChildElements(identificationElement[0], "versjonId",
                TillatAntall.MaksimaltEn);
            if (identificationVersionIds.Count == 1)
                identification.VersionId = identificationVersionIds[0].Value;

            return identification;
        }

        private static Quality ParseQuality(XElement xElement)
        {
            var quality = new Quality();

            if (xElement == null) return quality;
            var qualityElement =
                NinXmlParser.GetChildElements(xElement, "Posisjonskvalitet", TillatAntall.AkkuratEn);

            var qualityMeasuringMethods =
                NinXmlParser.GetChildElements(qualityElement[0], "maalemetode", TillatAntall.AkkuratEn);
            quality.MeasuringMethod = qualityMeasuringMethods[0].Value;

            var qualityAccuracies = NinXmlParser.GetChildElements(qualityElement[0], "noeyaktighet",
                TillatAntall.MaksimaltEn);
            if (qualityAccuracies.Count == 1)
                quality.Accuracy = int.Parse(qualityAccuracies[0].Value);

            var qualityVisibilities = NinXmlParser.GetChildElements(qualityElement[0], "synbarhet",
                TillatAntall.MaksimaltEn);
            if (qualityVisibilities.Count == 1)
                quality.Visibility = qualityVisibilities[0].Value;

            var qualityMeasuringMethodHeights = NinXmlParser.GetChildElements(qualityElement[0], "maalemetodeHoeyde",
                TillatAntall.MaksimaltEn);
            if (qualityMeasuringMethodHeights.Count == 1)
                quality.MeasuringMethodHeight = qualityMeasuringMethodHeights[0].Value;

            var qualityAccuracyHeights = NinXmlParser.GetChildElements(qualityElement[0], "noeyaktighetHoeyde",
                TillatAntall.MaksimaltEn);
            if (qualityAccuracyHeights.Count == 1)
                quality.AccuracyHeight = int.Parse(qualityAccuracyHeights[0].Value);

            var qualityMaxDerivations = NinXmlParser.GetChildElements(qualityElement[0], "maksimaltAvvik",
                TillatAntall.MaksimaltEn);
            if (qualityMaxDerivations.Count == 1)
                quality.MaxDeviation = int.Parse(qualityMaxDerivations[0].Value);

            return quality;
        }

        private static NinVariabelDefinisjon ParseVariableDefinition(XElement xElement)
        {
            NinVariabelDefinisjon definition;

            if (xElement == null) return null;
            var customVariableDefinitions = NinXmlParser.GetChildElements(xElement, "EgendefinertVariabelDefinisjon",
                TillatAntall.MaksimaltEn);
            var standardVariables = NinXmlParser.GetChildElements(xElement, "StandardisertVariabel",
                TillatAntall.MaksimaltEn);

            if (customVariableDefinitions.Count == 1)
            {
                definition = new CustomVariableDefinition();

                var customVariableDefinitionSpesifications = NinXmlParser.GetChildElements(customVariableDefinitions[0],
                    "betegnelse", TillatAntall.AkkuratEn);
                ((CustomVariableDefinition) definition).Specification = customVariableDefinitionSpesifications[0].Value;

                var customVariableDefinitionDescriptions = NinXmlParser.GetChildElements(customVariableDefinitions[0],
                    "beskrivelse", TillatAntall.AkkuratEn);
                ((CustomVariableDefinition) definition).Description = customVariableDefinitionDescriptions[0].Value;
            }
            else if (standardVariables.Count == 1)
            {
                definition = new NinStandardVariabel();

                var standardVariableDefinitions = NinXmlParser.GetChildElements(standardVariables[0],
                    "variabelDefinisjon", TillatAntall.AkkuratEn);
                ((NinStandardVariabel) definition).VariableDefinition =
                    NinXmlParser.ParseCode(standardVariableDefinitions[0]);
            }
            else
                throw new DataDeliveryParseException("The element " + xElement.Name.LocalName +
                                                     " contains incorrect variable definitions.");

            return definition;
        }

        private static Parameter ParseParameter(XElement xElement)
        {
            Parameter parameter;

            if (xElement == null) return null;
            var natureAreaTypes = NinXmlParser.GetChildElements(xElement, "NaturomraadeType",
                TillatAntall.MaksimaltEn);
            var descriptionVariables = NinXmlParser.GetChildElements(xElement, "Beskrivelsesvariabel",
                TillatAntall.MaksimaltEn);

            if (natureAreaTypes.Count == 1)
                parameter = ParseNatureAreaType(natureAreaTypes[0]);
            else if (descriptionVariables.Count == 1)
                parameter = ParseDescriptionVariable(descriptionVariables[0]);
            else
                throw new DataDeliveryParseException("The element " + xElement.Name.LocalName +
                                                     " contains incorrect parameters.");

            return parameter;
        }

        private static NatureAreaType ParseNatureAreaType(XElement xElement)
        {
            var natureAreaType = new NatureAreaType();

            if (xElement == null) return natureAreaType;
            var natureAreaTypeCodes = NinXmlParser.GetChildElements(xElement, "kode", TillatAntall.AkkuratEn);
            natureAreaType.Code = natureAreaTypeCodes[0].Value;

            var natureAreaTypeSurveyers =
                NinXmlParser.GetChildElements(xElement, "kartlegger", TillatAntall.MaksimaltEn);
            if (natureAreaTypeSurveyers.Count == 1)
                natureAreaType.Surveyer = NinXmlParser.ParseContact(natureAreaTypeSurveyers[0]);

            var natureAreaTypeSurveyeds =
                NinXmlParser.GetChildElements(xElement, "kartlagtDato", TillatAntall.MaksimaltEn);
            if (natureAreaTypeSurveyeds.Count == 1)
                natureAreaType.Surveyed = Convert.ToDateTime(natureAreaTypeSurveyeds[0].Value);

            var natureAreaTypeShares = NinXmlParser.GetChildElements(xElement, "andel", TillatAntall.AkkuratEn);
            natureAreaType.Share = double.Parse(natureAreaTypeShares[0].Value, CultureInfo.InvariantCulture);

            var natureAreaTypeCustomVariables = NinXmlParser.GetChildElements(xElement, "egendefinerteVariabler",
                TillatAntall.NullEllerFlere);
            foreach (var natureAreaTypeCustomVariable in natureAreaTypeCustomVariables)
                natureAreaType.CustomVariables.Add(ParseCustomVariable(natureAreaTypeCustomVariable));

            var natureAreaTypeAdditionalVariables = NinXmlParser.GetChildElements(xElement, "tilleggsVariabler",
                TillatAntall.NullEllerFlere);
            foreach (var natureAreaTypeAdditionalVariable in natureAreaTypeAdditionalVariables)
            {
                var natureAreaTypeAdditionalDescriptionVariables = NinXmlParser.GetChildElements(
                    natureAreaTypeAdditionalVariable, "Beskrivelsesvariabel", TillatAntall.AkkuratEn);
                natureAreaType.AdditionalVariables.Add(
                    ParseDescriptionVariable(natureAreaTypeAdditionalDescriptionVariables[0]));
            }

            return natureAreaType;
        }

        private static DescriptionVariable ParseDescriptionVariable(XElement xElement)
        {
            var descriptionVariable = new DescriptionVariable();

            if (xElement == null) return descriptionVariable;
            var natureAreaTypeCodes = NinXmlParser.GetChildElements(xElement, "kode", TillatAntall.AkkuratEn);
            (descriptionVariable).Code = natureAreaTypeCodes[0].Value;

            var descriptionVariableSurveyers =
                NinXmlParser.GetChildElements(xElement, "kartlegger", TillatAntall.MaksimaltEn);
            if (descriptionVariableSurveyers.Count == 1)
                descriptionVariable.Surveyer = NinXmlParser.ParseContact(descriptionVariableSurveyers[0]);

            var descriptionVariableSurveyeds =
                NinXmlParser.GetChildElements(xElement, "kartlagtDato", TillatAntall.MaksimaltEn);
            if (descriptionVariableSurveyeds.Count == 1)
                descriptionVariable.Surveyed = Convert.ToDateTime(descriptionVariableSurveyeds[0].Value);

            var descriptionVariableValues =
                NinXmlParser.GetChildElements(xElement, "verdi", TillatAntall.AkkuratEn);
            (descriptionVariable).Value = descriptionVariableValues[0].Value;

            var descriptionVariableDescriptions =
                NinXmlParser.GetChildElements(xElement, "beskrivelse", TillatAntall.MaksimaltEn);
            if (descriptionVariableDescriptions.Count == 1)
                (descriptionVariable).Description = descriptionVariableDescriptions[0].Value;
            return descriptionVariable;
        }

        private static CustomVariable ParseCustomVariable(XElement xElement)
        {
            var customVariable = new CustomVariable();

            if (xElement == null) return customVariable;
            var customVariableElements =
                NinXmlParser.GetChildElements(xElement, "EgendefinertVariabel", TillatAntall.AkkuratEn);

            var customVariableSpecifications = NinXmlParser.GetChildElements(customVariableElements[0], "betegnelse",
                TillatAntall.AkkuratEn);
            customVariable.Specification = customVariableSpecifications[0].Value;

            var customVariableValues =
                NinXmlParser.GetChildElements(customVariableElements[0], "verdi", TillatAntall.AkkuratEn);
            customVariable.Value = customVariableValues[0].Value;

            return customVariable;
        }

        private static List<NatureArea> ParseNatureArea(XElement xElement)
        {
            var natureAreaElement =
                NinXmlParser.GetChildElements(xElement, "NaturOmraade", TillatAntall.EnEllerFlere);

            List<NatureArea> r = new List<NatureArea>();
            foreach (var na in natureAreaElement)
                r.Add(ParseNatureArea2(na));
            return r;
        }

        private static NatureArea ParseNatureArea2(XElement element)
        {
            var natureArea = new NatureArea();

            var natureAreaUniqueIds =
                NinXmlParser.GetChildElements(element, "unikId", TillatAntall.AkkuratEn);
            natureArea.UniqueId = ParseIdentification(natureAreaUniqueIds[0]);

            var natureAreaVersions =
                NinXmlParser.GetChildElements(element, "versjon", TillatAntall.AkkuratEn);
            natureArea.Version = natureAreaVersions[0].Value;

            var natureAreaNatureLevels =
                NinXmlParser.GetChildElements(element, "nivaa", TillatAntall.AkkuratEn);
            var xNaturnivå = natureAreaNatureLevels[0];
            switch (xNaturnivå.Value)
            {
                case "1":
                    natureArea.Nivå = NatureLevel.Landskapstype;
                    break;
                case "2":
                    natureArea.Nivå = NatureLevel.Landskapsdel;
                    break;
                case "3":
                    natureArea.Nivå = NatureLevel.Naturkompleks;
                    break;
                case "4":
                    natureArea.Nivå = NatureLevel.Natursystem;
                    break;
                case "5":
                    natureArea.Nivå = NatureLevel.Naturkomponent;
                    break;
                case "6":
                    natureArea.Nivå = NatureLevel.Livsmedium;
                    break;
                case "7":
                    natureArea.Nivå = NatureLevel.KnowledgeArea;
                    break;
                default:
                    throw new DataDeliveryParseException("The element " + xNaturnivå.Name.LocalName +
                                                         " contains a unknown value.");
            }

            var natureAreaAreas =
                NinXmlParser.GetChildElements(element, "omraade", TillatAntall.AkkuratEn);
            int natureAreaEpsgCode;
            natureArea.Area = NinXmlParser.ParseGeometry(natureAreaAreas[0], out natureAreaEpsgCode);
            natureArea.AreaEpsgCode = natureAreaEpsgCode;

            var natureAreaSurveyers = NinXmlParser.GetChildElements(element, "kartlegger",
                TillatAntall.MaksimaltEn);
            if (natureAreaSurveyers.Count == 1)
                natureArea.Surveyer = NinXmlParser.ParseContact(natureAreaSurveyers[0]);
            var natureAreaSurveyeds = NinXmlParser.GetChildElements(element, "kartlagtDato",
                TillatAntall.MaksimaltEn);
            if (natureAreaSurveyeds.Count == 1)
                natureArea.Surveyed = Convert.ToDateTime(natureAreaSurveyeds[0].Value);

            var natureAreaDescriptions = NinXmlParser.GetChildElements(element, "beskrivelse",
                TillatAntall.MaksimaltEn);
            if (natureAreaDescriptions.Count == 1)
                natureArea.Description = natureAreaDescriptions[0].Value;

            var natureAreaDocuments = NinXmlParser.GetChildElements(element, "dokumenter",
                TillatAntall.NullEllerFlere);
            foreach (var natureAreaDocument in natureAreaDocuments)
                natureArea.Documents.Add(NinXmlParser.ParseDocument(natureAreaDocument));

            var natureAreaParameters = NinXmlParser.GetChildElements(element, "parametre",
                TillatAntall.EnEllerFlere);
            foreach (var natureAreaParameter in natureAreaParameters)
                natureArea.Parameters.Add(ParseParameter(natureAreaParameter));

            return natureArea;
        }

        private static Metadata ParseMetadata(XElement xElement)
        {
            var metadata = new Metadata();

            if (xElement == null) return metadata;
            var metadataElement = NinXmlParser.GetChildElements(xElement, "Metadata", TillatAntall.AkkuratEn);

            var e0 = metadataElement[0];
            var metadataUniqueIds =
                NinXmlParser.GetChildElements(e0, "unikId", TillatAntall.AkkuratEn);
            metadata.UniqueId = ParseIdentification(metadataUniqueIds[0]);

            var metadataPrograms =
                NinXmlParser.GetChildElements(e0, "program", TillatAntall.AkkuratEn);
            metadata.Program = metadataPrograms[0].Value;

            var metadataProjectNames =
                NinXmlParser.GetChildElements(e0, "prosjektnavn", TillatAntall.AkkuratEn);
            metadata.ProjectName = metadataProjectNames[0].Value;

            var metadataProjectDescriptions = NinXmlParser.GetChildElements(e0, "prosjektbeskrivelse",
                TillatAntall.MaksimaltEn);
            if (metadataProjectDescriptions.Count == 1)
                metadata.ProjectDescription = metadataProjectDescriptions[0].Value;

            var metadataPurposes = NinXmlParser.GetChildElements(e0, "formaal",
                TillatAntall.MaksimaltEn);
            if (metadataPurposes.Count == 1)
                metadata.Purpose = metadataPurposes[0].Value;

            var metadataContractors =
                NinXmlParser.GetChildElements(e0, "oppdragsgiver", TillatAntall.AkkuratEn);
            metadata.Contractor = NinXmlParser.ParseContact(metadataContractors[0]);

            var metadataOwners = NinXmlParser.GetChildElements(e0, "eier", TillatAntall.AkkuratEn);
            metadata.Owner = NinXmlParser.ParseContact(metadataOwners[0]);

            var metadataSurveyedFromDates = NinXmlParser.GetChildElements(e0, "kartlagtFraDato",
                TillatAntall.AkkuratEn);
            metadata.SurveyedFrom = Convert.ToDateTime(metadataSurveyedFromDates[0].Value);

            var metadataSurveyedToDates =
                NinXmlParser.GetChildElements(e0, "kartlagtTilDato", TillatAntall.AkkuratEn);
            metadata.SurveyedTo = Convert.ToDateTime(metadataSurveyedToDates[0].Value);

            var metadataSurveyScales = NinXmlParser.GetChildElements(e0, "kartleggingsMaalestokk",
                TillatAntall.AkkuratEn);
            metadata.SurveyScale = metadataSurveyScales[0].Value;

            var metadataResolutions = NinXmlParser.GetChildElements(e0, "opploesning",
                TillatAntall.MaksimaltEn);
            if (metadataResolutions.Count == 1)
                metadata.Resolution = metadataResolutions[0].Value;

            var metadataAreas =
                NinXmlParser.GetChildElements(e0, "dekningsOmraade", TillatAntall.AkkuratEn);
            int metadataAreaEspgCode;
            metadata.Area = NinXmlParser.ParseGeometry(metadataAreas[0], out metadataAreaEspgCode);
            metadata.AreaEpsgCode = metadataAreaEspgCode;

            var metadataQualities =
                NinXmlParser.GetChildElements(e0, "kvalitet", TillatAntall.AkkuratEn);
            metadata.Quality = ParseQuality(metadataQualities[0]);

            var metadataDocuments = NinXmlParser.GetChildElements(e0, "dokumenter",
                TillatAntall.NullEllerFlere);
            foreach (var metadataDocument in metadataDocuments)
                metadata.Documents.Add(NinXmlParser.ParseDocument(metadataDocument));

            var metadataNatureAreas = NinXmlParser.GetChildElements(e0, "naturOmraader",
                TillatAntall.EnEllerFlere);
            foreach (var metadataNatureArea in metadataNatureAreas)
                metadata.NatureAreas.AddRange(ParseNatureArea(metadataNatureArea));

            var metadataVariableDefinitions = NinXmlParser.GetChildElements(e0, "variabelDefinisjoner",
                TillatAntall.EnEllerFlere);
            foreach (var metadataVariableDefinition in metadataVariableDefinitions)
                metadata.VariabelDefinitions.Add(ParseVariableDefinition(metadataVariableDefinition));

            return metadata;
        }
    }
}