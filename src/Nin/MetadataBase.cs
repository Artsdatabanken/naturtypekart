using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Nin.Dataleveranser;

namespace Nin.Types
{
    public abstract class MetadataBase
    {
        public MetadataBase() { }

        public MetadataBase(MetadataBase metadata)
        {
            Program = metadata.Program;
            ProjectName = metadata.ProjectName;
            ProjectDescription = metadata.ProjectDescription;
            Purpose = metadata.Purpose;
            SurveyedFrom = metadata.SurveyedFrom;
            SurveyedTo = metadata.SurveyedTo;
            SurveyScale = metadata.SurveyScale;
            Resolution = metadata.Resolution;
        }

        public string Program { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string Purpose { get; set; }
        public DateTime? SurveyedFrom { get; set; }
        public DateTime? SurveyedTo { get; set; }
        public string SurveyScale { get; set; }
        public string Resolution { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class Metadata : Types.MetadataBase
    {
        public Metadata() { }

        public Metadata(MsSql.Metadata metadata) : base(metadata)
        {
            if (metadata.UniqueId != null) UniqueId = new Identification(metadata.UniqueId);
            if (metadata.Contractor != null) Contractor = new Contact(metadata.Contractor);
            if (metadata.Owner != null) Owner = new Contact(metadata.Owner);

            Area = metadata.Area.ToString();
            AreaEpsgCode = metadata.Area.STSrid.Value;

            if (metadata.Quality != null) Quality = new Quality(metadata.Quality);
            foreach (var document in metadata.Documents)
                Documents.Add(new Document(document));
            foreach (var natureArea in metadata.NatureAreas)
                NatureAreas.Add(new NatureArea(natureArea));
            foreach (var variabelDefinition in metadata.VariabelDefinitions)
            {
                if (variabelDefinition.GetType() == typeof(MsSql.NinStandardVariabel))
                    VariabelDefinitions.Add(new NinStandardVariabel((MsSql.NinStandardVariabel)variabelDefinition));
                else
                    VariabelDefinitions.Add(
                        new CustomVariableDefinition((MsSql.CustomVariableDefinition)variabelDefinition));
            }
        }

        public Identification UniqueId { get; set; }
        public Contact Contractor { get; set; }
        public Contact Owner { get; set; }
        public string Area { get; set; }
        public int AreaEpsgCode { get; set; }
        public Quality Quality { get; set; }

        public Collection<Document> Documents { get; } = new Collection<Document>();
        public Collection<NatureArea> NatureAreas { get; } = new Collection<NatureArea>();
        public Collection<NinVariabelDefinisjon> VariabelDefinitions { get; } = new Collection<NinVariabelDefinisjon>();

        public Document FindDocument(string fileName)
        {
            Document _document = null;

            foreach (var natureArea in NatureAreas)
            {
                foreach (var document in natureArea.Documents)
                {
                    if (document.FileName != fileName) continue;
                    if (_document != null)
                        throw new Exception("Dokument '" + fileName + "' er angitt flere ganger.");
                    _document = document;
                }
            }

            foreach (var document in Documents)
            {
                if (document.FileName != fileName) continue;
                if (_document != null)
                    throw new Exception("Dokument '" + fileName + "' er angitt flere ganger.");
                _document = document;
            }

            return _document;
        }
    }
}

namespace Nin.Types.MsSql
{
    public class Metadata : MetadataBase
    {
        public Metadata() { }

        public Metadata(RavenDb.Metadata metadata) : base(metadata)
        {
            if (metadata.UniqueId != null) UniqueId = new Identification(metadata.UniqueId);
            if (metadata.Contractor != null) Contractor = new Contact(metadata.Contractor);
            if (metadata.Owner != null) Owner = new Contact(metadata.Owner);

            Area = SqlGeometry.STGeomFromText(
                new SqlChars(metadata.Area),
                metadata.AreaEpsgCode
            );

            if (metadata.Quality != null) Quality = new Quality(metadata.Quality);
            foreach (var document in metadata.Documents)
                Documents.Add(new Document(document));
            foreach (var natureArea in metadata.NatureAreas)
                NatureAreas.Add(new NatureArea(natureArea));
            foreach (var variabelDefinition in metadata.VariabelDefinitions)
            {
                if (variabelDefinition.GetType() == typeof(RavenDb.NinStandardVariabel))
                    VariabelDefinitions.Add(new NinStandardVariabel((RavenDb.NinStandardVariabel)variabelDefinition));
                else
                    VariabelDefinitions.Add(
                        new CustomVariableDefinition((RavenDb.CustomVariableDefinition)variabelDefinition));
            }
        }

        [JsonIgnore]
        public int Id { get; set; }

        public Identification UniqueId { get; set; }
        public Contact Contractor { get; set; }
        public Contact Owner { get; set; }

        [JsonIgnore]
        public SqlGeometry Area { get; set; }
        [JsonProperty("area")]
        public string AreaWkt { get { return Area?.STAsText().ToSqlString().Value; } }

        public Quality Quality { get; set; }

        public Collection<Document> Documents { get; set; } = new Collection<Document>();
        public Collection<NatureArea> NatureAreas { get; set; } = new Collection<NatureArea>();
        public List<NinVariabelDefinisjon> VariabelDefinitions { get; set; } = new List<NinVariabelDefinisjon>();

        public int GetAreaEpsgCode()
        {
            return Area?.STSrid.Value ?? 0;
        }
    }
}
