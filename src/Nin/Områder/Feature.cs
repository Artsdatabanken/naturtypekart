using System.Collections.Generic;

namespace Nin.Områder
{
    public class Feature
    {
        public Dictionary<string, string> Properties { get; set; }
        public Polygon Geometry { get; set; }
    }
}
