using Microsoft.SqlServer.Types;
using Newtonsoft.Json;

namespace Nin.Områder
{
    public class Area
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Unique number
        /// </summary>
        public int Number { get; set; }

        public AreaType Type { get; set; }
        public SqlGeometry Geometry { get; set; }
        public string Category { get; set; }

        public string Value { get; set; }
    }
}