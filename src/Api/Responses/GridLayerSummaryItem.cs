using System;
using System.Collections.ObjectModel;
using Nin.Types.MsSql;

namespace Nin.Api.Responses
{
    public class GridLayerSummaryItem
    {
        public int Id { get; set; }
        public Guid DocGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Established { get; set; }

        public string MinValue { get; set; }
        public string MaxValue { get; set; }

        public string Code { get; set; }
        public string CodeDescription { get; set; }
        public string CodeUrl { get; set; }

        public Contact Owner { get; set; }
        public Collection<Document> Documents { get; set; }

        public GridLayerSummaryItem()
        {
            Documents = new Collection<Document>();
        }
    }
}
