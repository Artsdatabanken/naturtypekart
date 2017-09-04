using System;
using System.Collections.ObjectModel;
using Nin.Områder;
using Nin.Types.RavenDb;

namespace Nin.Dataleveranser.Rutenett
{
    public class GridDelivery
    {
        public GridDelivery()
        {
            AreaType = AreaType.Undefined;
            RutenettType = RutenettType.Undefined;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Code Code { get; set; }
        public Contact Owner { get; set; }
        public DateTime Established { get; set; }

        public AreaType AreaType { get; set; }
        public RutenettType RutenettType { get; set; }

        public Collection<Document> Documents { get; } = new Collection<Document>();

        public string DocumentDescription { get; set; }

        public string Username { get; set; }

        public void MapAreaTypeNumber(string aoType)
        {
            if (string.IsNullOrEmpty(aoType)) return;
            if (aoType == "undefined") return;

            AreaType = (AreaType) int.Parse(aoType);
        }

        public void MapGridTypeNumber(string ssbType)
        {
            if (string.IsNullOrEmpty(ssbType)) return;
            if (ssbType == "undefined") return;

            RutenettType = (RutenettType) int.Parse(ssbType);
        }
    }
}