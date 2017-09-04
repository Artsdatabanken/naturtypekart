using System;
using Newtonsoft.Json;
using Types;

namespace Nin.Types
{
    public abstract class DataleveranseBase
    {
        public DataleveranseBase()
        {
        }

        public DataleveranseBase(DataleveranseBase dataleveranse)
        {
            Id = dataleveranse.Id;
            Name = dataleveranse.Name;
            DeliveryDate = dataleveranse.DeliveryDate;
            ReasonForChange = dataleveranse.ReasonForChange;
            Description = dataleveranse.Description;
            ParentId = dataleveranse.ParentId;
            Created = dataleveranse.Created;
            Expired = dataleveranse.Expired;
            Publisering = dataleveranse.Publisering;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string ReasonForChange { get; set; }
        public string Description { get; set; }

        public string ParentId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Expired { get; set; }
        public Status Publisering { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class Dataleveranse : DataleveranseBase
    {
        public Contact Operator { get; set; }
        public Metadata Metadata { get; set; }

        public string Username { get; set; }
    }
}

namespace Nin.Types.MsSql
{
    public class Dataleveranse : DataleveranseBase
    {
        public Dataleveranse()
        {
        }

        public Dataleveranse(RavenDb.Dataleveranse dataleveranse) : base(dataleveranse)
        {
            if (dataleveranse.Operator != null) Operator = new Contact(dataleveranse.Operator);
            if (dataleveranse.Metadata != null) Metadata = new Metadata(dataleveranse.Metadata);
        }

        [JsonIgnore]
        public int DataId { get; set; }

        public Contact Operator { get; set; }
        public Metadata Metadata { get; set; }
    }
}