using System;
using System.Linq;
using DotSpatial.Topology;
using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Områder;
using Nin.Tasks;
using Point = Nin.Map.Tiles.Geometri.Point;

namespace Nin.Map.Layers
{
    public class TiledVectorLayer
    {
        private void InitializeResolutions(double resolutionAtZoom0)
        {
            _resolutions = new double[MaximumZoomLevel + 1];
            for (var i = 0; i <= MaximumZoomLevel; i++)
                _resolutions[i] = resolutionAtZoom0*Math.Pow(0.5, i);
        }

        public int GetZForResolution(double resolution)
        {
            if (_resolutions == null)
                InitializeResolutions(ResolutionAtZoom0);

            var z = Array.FindIndex(_resolutions, x => x <= resolution);
            return z < 0 ? MaximumZoomLevel : z;
        }

        public double GetResolution(int z)
        {
            if (_resolutions == null)
                InitializeResolutions(ResolutionAtZoom0);

            return _resolutions[z];
        }

        public Point GetOrigin(int zoomLevel)
        {
            var origin = new Point(BoundingBox.Left, BoundingBox.Top);
            return origin;
        }

        public TiledVectorLayer(string layerName, BoundingBox bbox, double resolutionAtZoom0)
        {
            BoundingBox = bbox;
            Name = layerName;
            ResolutionAtZoom0 = resolutionAtZoom0;
        }

        public override string ToString()
        {
            return
                $"{Name} Zoom {MinimumZoomLevel}-{MaximumZoomLevel} Overlap {TileOverlapRatio} Simplification {SimplificationToleranceAtZoom0}";
        }

        public const int TileSize = 256;
        public string Name;

        private int MinimumZoomLevel => ZoomFactors.Min(x => x.Minimum);
        private int MaximumZoomLevel => ZoomFactors.Max(x => x.Maximum);

        public BoundingBox BoundingBox;
        public double TileOverlapRatio = 0;
        public double ResolutionAtZoom0;
        public ZoomFactor[] ZoomFactors = {new ZoomFactor(AreaType.Undefined, 5,11) };

        private double[] _resolutions;
        public double SimplificationToleranceAtZoom0 = 1e7;

        public IEnvelope GetEnvelope()
        {
            return new Envelope(BoundingBox.Left, BoundingBox.Right, BoundingBox.Bottom, BoundingBox.Top);
        }

        public ZoomFactor GetZoomFactors(AreaType areaType)
        {
            foreach (var zf in ZoomFactors)
            {
                if (zf.AreaType == areaType) return zf;
                if (zf.AreaType == AreaType.Undefined) return zf;
            }
            throw new Exception($"Mangler konfigurasjon av zoom faktorer for områdetype \'{areaType}\'.");
        }
    }
}