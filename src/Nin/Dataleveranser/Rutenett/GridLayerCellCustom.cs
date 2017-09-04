using Microsoft.SqlServer.Types;
using Nin.Types.GridTypes;

namespace Nin.Dataleveranser.Rutenett
{
    public class GridLayerCellCustom : GridLayerCell
    {
        public SqlGeometry CustomCell { get; set; }
    }
}
