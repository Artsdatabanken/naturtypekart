using System.Collections.ObjectModel;
using Nin.Types.GridTypes;

namespace Nin.Områder
{
    public class AreaLayer : Layer
    {
        public AreaType Type { get; set; }
        public Collection<AreaLayerItem> Items { get; set; } = new Collection<AreaLayerItem>();

        public AreaLayer()
        {
        }

        public AreaLayer(string name, AreaType type)
        {
            Name = name;
            Type = type;
        }
    }
}
