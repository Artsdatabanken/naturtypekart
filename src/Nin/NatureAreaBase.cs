using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Nin.Områder;
using Types;

namespace Nin.Types
{
    public abstract class NatureAreaBase
    {
        protected NatureAreaBase() { }
        protected NatureAreaBase(NatureAreaBase natureArea)
        {
            Version = natureArea.Version;
            Nivå = natureArea.Nivå;
            Surveyed = natureArea.Surveyed;
            Description = natureArea.Description;
        }

        public string Version { get; set; }
        public NatureLevel Nivå { get; set; }
        public DateTime? Surveyed { get; set; }
        public string Description { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class NatureArea : NatureAreaBase
    {
        public NatureArea() { }

        public NatureArea(MsSql.NatureArea natureArea) : base(natureArea)
        {
            if (natureArea.UniqueId != null) UniqueId = new Identification(natureArea.UniqueId);

            Area = natureArea.Area.ToString();
            AreaEpsgCode = natureArea.Area.STSrid.Value;

            if (natureArea.Surveyer != null) Surveyer = new Contact(natureArea.Surveyer);
            foreach (var document in natureArea.Documents)
                Documents.Add(new Document(document));
            foreach (var parameter in natureArea.Parameters)
            {
                if (parameter.GetType() == typeof(NatureAreaType))
                    Parameters.Add(new NatureAreaType((MsSql.NatureAreaType) parameter));
                else
                    Parameters.Add(new DescriptionVariable((MsSql.DescriptionVariable) parameter));
            }
        }

        public Identification UniqueId { get; set; }
        public string Area { get; set; }
        public int AreaEpsgCode { get; set; }
        public Contact Surveyer { get; set; }

        public Collection<Document> Documents { get; set; } = new Collection<Document>();
        public Collection<Parameter> Parameters { get; set; } = new Collection<Parameter>();
    }

    public class NatureAreaExport : NatureArea
    {
        public NatureAreaExport(NatureArea natureArea)
        {
            Version = natureArea.Version;
            Nivå = natureArea.Nivå;
            Surveyed = natureArea.Surveyed;
            Description = natureArea.Description;
            UniqueId = natureArea.UniqueId;
            Area = natureArea.Area;
            AreaEpsgCode = natureArea.AreaEpsgCode;
            Surveyer = natureArea.Surveyer;
            Documents = natureArea.Documents;
            Parameters = natureArea.Parameters;
        }

        public Collection<Area> Areas { get; set; } = new Collection<Area>();
    }
}

namespace Nin.Types.MsSql
{
    public class NatureArea : NatureAreaBase
    {
        public NatureArea() { }

        public NatureArea(RavenDb.NatureArea natureArea) : base(natureArea)
        {
            if (natureArea.UniqueId != null) UniqueId = new Identification(natureArea.UniqueId);

            Area = SqlGeometry.STGeomFromText(
                new SqlChars(natureArea.Area), 
                natureArea.AreaEpsgCode
            );

            if (natureArea.Surveyer != null) Surveyer = new Contact(natureArea.Surveyer);
            foreach (var document in natureArea.Documents)
                Documents.Add(new Document(document));
            foreach (var parameter in natureArea.Parameters)
            {
                if (parameter.GetType() == typeof(RavenDb.NatureAreaType))
                   Parameters.Add(new NatureAreaType((RavenDb.NatureAreaType) parameter));
                else
                   Parameters.Add(new DescriptionVariable((RavenDb.DescriptionVariable) parameter));
            }
        }

        [JsonIgnore]
        public int Id { get; set; }

        public Identification UniqueId { get; set; }

        [JsonIgnore]
        public SqlGeometry Area { get; set; }
        public Contact Surveyer { get; set; }

        public Collection<Document> Documents { get; set; } = new Collection<Document>();
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        public string Institution { get; set; }
    }

    public class NatureAreaExport : NatureArea
    {
        public NatureAreaExport() {}

        public NatureAreaExport(NatureArea natureArea)
        {
            Version = natureArea.Version;
            Nivå = natureArea.Nivå;
            Surveyed = natureArea.Surveyed;
            Description = natureArea.Description;
            UniqueId = natureArea.UniqueId;
            Area = natureArea.Area;
            Surveyer = natureArea.Surveyer;
            Documents = natureArea.Documents;
            Parameters = natureArea.Parameters;
            Institution = natureArea.Institution;
        }

        public string MetadataSurveyScale { get; set; }
        public string MetadataContractor { get; set; }
        public string MetadataProgram { get; set; }
    }
}