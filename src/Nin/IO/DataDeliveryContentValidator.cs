using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Nin.Common;
using Nin.Dataleveranser;
using Nin.Naturtyper;
using Nin.Types;
using Nin.Types.RavenDb;
using Types;
using DescriptionVariable = Nin.Types.RavenDb.DescriptionVariable;
using NatureArea = Nin.Types.RavenDb.NatureArea;
using NatureAreaType = Nin.Types.RavenDb.NatureAreaType;
using NinStandardVariabel = Nin.Types.RavenDb.NinStandardVariabel;

namespace Nin.IO
{
    public class DataDeliveryContentValidator
    {
        private readonly SqlGeometry norwayExtents;

        public DataDeliveryContentValidator()
        {
            norwayExtents = SqlGeometry.STGeomFromText(
                new SqlChars("POLYGON((-500000 6000000, -500000 8500000, 1500000 8500000, 1500000 6000000, -500000 6000000))"), 32633);
        }

        public void ValidateDataDeliveryContent(Dataleveranse dataleveranse)
        {
            ValidateDefinitionsAndParameters(dataleveranse.Metadata.VariabelDefinitions, dataleveranse.Metadata.NatureAreas);
            ValidateGeometries(dataleveranse);
        }

        private static void ValidateDefinitionsAndParameters(IReadOnlyCollection<NinVariabelDefinisjon> variableDefinitions, IEnumerable<NatureArea> natureAreas)
        {
            foreach (var natureArea in natureAreas)
            {
                string natureLevelCode = Naturnivå.TilKode(natureArea.Nivå);

                foreach (var parameter in natureArea.Parameters)
                {
                    Validate(variableDefinitions, parameter, natureLevelCode);
                }
            }
        }

        private static void Validate(IEnumerable<NinVariabelDefinisjon> variableDefinitions, Parameter parameter, string naturnivåkode)
        {
            var natureAreaType = parameter as NatureAreaType;
            if (natureAreaType != null)
            {
                Validate(variableDefinitions, naturnivåkode, natureAreaType);
                return;
            }

            var variable = parameter as DescriptionVariable;
            if (variable != null)
            {
                var descriptionVariable = variable;
                // TODO: Validate description variable codes when we have a working web service for this codes.

                ValidateStandardVariableDefinition(variableDefinitions, descriptionVariable);
            }
        }

        private static void Validate(IEnumerable<NinVariabelDefinisjon> variableDefinitions, string natureLevelCode, NatureAreaType natureAreaType)
        {
            if (Naturkodetrær.Naturtyper.HentFraKode(natureAreaType.Code) == null)
                throw new Exception("Ukjent Nin-kode: " + natureAreaType.Code);

            if (!natureAreaType.Code.Substring(0, 2).Equals(natureLevelCode))
                throw new Exception("Nature area type code " +
                                                                natureAreaType.Code +
                                                                " can not be a part of a nature area with nature level code " +
                                                                natureLevelCode);

            ValidateStandardVariableDefinition(variableDefinitions, natureAreaType);
        }

        private static void ValidateStandardVariableDefinition(IEnumerable<NinVariabelDefinisjon> variableDefinitions, Parameter parameter)
        {
            foreach (var variableDefinition in variableDefinitions)
            {
                if (variableDefinition.GetType() != typeof(NinStandardVariabel)) continue;
                var standardVariable = (NinStandardVariabel)variableDefinition;
                if (standardVariable.VariableDefinition.Value == parameter.Code)
                    return;
            }
            //throw new DataDeliveryContentValidatorException("Ukjent Nin-kode: " + parameter.Code);
        }

        private void ValidateGeometries(Dataleveranse dataleveranse)
        {
            var metadataGeometry = SqlGeometry.STGeomFromText(new SqlChars(dataleveranse.Metadata.Area), dataleveranse.Metadata.AreaEpsgCode);

            var bounds = GetExtent(dataleveranse.Metadata.AreaEpsgCode);

            if (!bounds.STContains(metadataGeometry))
                throw new Exception("Område ligger utenfor Norge.");

            foreach (var natureArea in dataleveranse.Metadata.NatureAreas)
            {
                var natureAreaGeometry = SqlGeometry.STGeomFromText(new SqlChars(natureArea.Area), natureArea.AreaEpsgCode);

                if (!metadataGeometry.STContains(natureAreaGeometry))
                    throw new Exception(
                        "Metadata area does not contain the area in nature area with id: " + natureArea.UniqueId.LocalId);
            }
        }

        private SqlGeometry GetExtent(int epsgCode)
        {
            if (epsgCode == norwayExtents.STSrid)
                return norwayExtents;
            return new MapProjection(epsgCode).Reproject(norwayExtents);
        }
    }
}
