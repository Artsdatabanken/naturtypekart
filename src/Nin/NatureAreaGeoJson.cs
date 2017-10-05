using Microsoft.SqlServer.Types;
using Nin.Types.MsSql;
using System.Collections.Generic;
using Types;

namespace Nin
{
    public class NatureAreaGeoJson : INatureAreaGeoJson
    {
        public int Id { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public Identification UniqueId { get; set; }
        public SqlGeometry Area { get; set; }
        public int Count { get; set; }
    }
}