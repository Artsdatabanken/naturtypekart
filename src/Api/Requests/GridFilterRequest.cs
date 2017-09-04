using System.Collections.ObjectModel;

namespace Nin.Api.Requests
{
    public class GridFilterRequest
    {
        public string GridType { get; set; }
        public Collection<int> Municipalities { get; set; }
        public Collection<int> Counties { get; set; }
        public string Geometry { get; set; }
        public string BoundingBox { get; set; }
        public int EpsgCode { get; set; }
        public int GridLayerTypeId { get; set; }
    }
}
