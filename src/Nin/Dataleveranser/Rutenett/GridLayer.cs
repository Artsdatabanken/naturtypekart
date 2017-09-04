using System;
using System.Collections.ObjectModel;
using Nin.Dataleveranser.Rutenett;

namespace Nin.Types.GridTypes
{
    public class Layer
    {
        public int Id { get; set; }
        public Guid DocGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Code Code { get; set; }
        public MsSql.Contact Owner { get; set; } = new MsSql.Contact();
        public DateTime Established { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public Collection<MsSql.Document> Documents { get; set; } = new Collection<MsSql.Document>();
    }

    public class GridLayer : Layer
    {
        public RutenettType Type { get; set; }
        public Collection<GridLayerCell> Cells { get; set; } = new Collection<GridLayerCell>();

        public GridLayer()
        {
        }

        public GridLayer(string name, RutenettType type)
        {
            Name = name;
            Type = type;
        }
    }
}
