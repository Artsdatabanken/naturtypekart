using Microsoft.SqlServer.Types;

namespace Nin.Types.GridTypes
{
    public class GridCell
    {
        public int Id { get; set; }
        public string CellId { get; set; }
        public SqlGeometry Geometry { get; set; }
        public string Value { get; set; }
    }
}
