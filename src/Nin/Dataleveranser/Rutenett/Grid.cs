using System.Collections.ObjectModel;
using Nin.Dataleveranser.Rutenett;
using Nin.Types.GridTypes;

namespace Common.Rutenett
{
    public class Grid
    {
        public RutenettType Type { get; set; }
        public Collection<GridCell> Cells { get; set; }

        public Grid(RutenettType type)
        {
            Type = type;
            Cells = new Collection<GridCell>();
        }
    }
}