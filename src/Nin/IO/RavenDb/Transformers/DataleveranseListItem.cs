using System;
using Types;

namespace Nin.IO.RavenDb.Transformers
{
    public class DataleveranseListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public string Username { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Expired { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string OperatorCompany { get; set; }
        public string OperatorContactPerson { get; set; }
        public string OperatorEmail { get; set; }
        public string OperatorHomesite { get; set; }
        public string MetadataProjectDescription { get; set; }
        public string MetadataProjectName { get; set; }
    }
}