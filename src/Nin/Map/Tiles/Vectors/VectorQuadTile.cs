using System;
using System.Collections.Generic;
using DotSpatial.Data;
using DotSpatial.Topology;
using Nin.Common.Map.Geometric.BoundingBoxes;
using Nin.Configuration;
using Nin.Diagnostic;
using Nin.Map.Layers;
using Nin.Områder;
using Feature = DotSpatial.Data.Feature;
using Geometry = DotSpatial.Topology.Geometry;
using GeometryFactory = DotSpatial.Topology.GeometryFactory;
using LineString = DotSpatial.Topology.LineString;
using MultiPoint = DotSpatial.Topology.MultiPoint;

namespace Nin.Map.Tiles.Vectors
{
    public class VectorQuadTile : QuadTile
    {
        public VectorQuadTile(BoundingBox boundingBox, int zoomLevel, int x, int y) : base(boundingBox, zoomLevel, x, y)
        {
        }

        protected override QuadTile Create(double left, double top, double right, double bottom, int dx, int dy)
        {
            return new VectorQuadTile(new BoundingBox(left, top, right, bottom),
                TileCoordinates.ZoomLevel + 1, TileCoordinates.X * 2 + dx, TileCoordinates.Y * 2 + dy);
        }

        public bool ClipAndAdd(Geometri.Område område, Geometry geometry, double marginFactor)
        {
            if (!geometry.Envelope.Intersects(GetEnvelope()))
                Log.w("TILE", $"Tried to update tile {TileCoordinates}, but nothing to do for {område.Type} Id {område.AreaId}");

            var envelope = GetBufferedEnvelope(marginFactor);
            var clip = Intersect(new Feature(geometry), envelope);
            if (clip == null || clip.IsEmpty) return false;

            UpdateGeometry(område, clip);

            VisualizeEnvelope();
            return true;
        }

        private void VisualizeEnvelope()
        {
            if (!Config.Settings.Map.StoreBoundingBoxes) return;
            var coordinates = new[]
            {
                new Coordinate(BoundingBox.Left, BoundingBox.Bottom),
                new Coordinate(BoundingBox.Left, BoundingBox.Top),
                new Coordinate(BoundingBox.Right, BoundingBox.Top),
                new Coordinate(BoundingBox.Right, BoundingBox.Top),
                new Coordinate(BoundingBox.Right, BoundingBox.Bottom),
                new Coordinate(BoundingBox.Left, BoundingBox.Bottom)
            };
            var geom = new LineString(coordinates);

            var bbox = new Geometri.Område(-2, AreaType.BoundingBox)
            {
                Number = -2,
                Name = "envelope",
                kind = "envelope"
            };
            UpdateGeometry(bbox, geom);
        }

        private static Geometry Intersect(IFeature f, IGeometry envelope)
        {
            Geometry clip;
            if (f.GeometryType == "GeometryCollection")
            {
                // This hack because DotSpatial 1.9 throws NullReferenceException when trying to Intersect GeometryCollection
                List<IBasicGeometry> arr = new List<IBasicGeometry>();
                for (int i = 0; i < f.NumGeometries; i++)
                {
                    IBasicGeometry g = f.GetBasicGeometryN(i);
                    IFeature intersection = new Feature(g).Intersection(envelope);
                    if (intersection == null) continue;
                    clip = (Geometry)intersection.BasicGeometry;
                    arr.Add(clip);
                }
                clip = new GeometryCollection(arr, new GeometryFactory());
            }
            else
            {
                IFeature intersection = f.Intersection(envelope);
                if (intersection == null) return null;
                clip = (Geometry)intersection.BasicGeometry;
            }
            return clip;
        }

        private void UpdateGeometry(Geometri.Område område, Geometry geometry)
        {
            foreach (OmrådeMedGeometry t in Områder)
            {
                if (t.Område.AreaId != område.AreaId) continue;
                t.Geometry = geometry;
                return;
            }
            Områder.Add(new OmrådeMedGeometry(område, geometry));
        }

        public IEnvelope GetEnvelope()
        {
            return new Envelope(BoundingBox.Left, BoundingBox.Right, BoundingBox.Bottom, BoundingBox.Top);
        }
        
        private IGeometry GetBufferedEnvelope(double marginFactor)
        {
            double marginX = (BoundingBox.Right - BoundingBox.Left) * marginFactor;
            double marginY = (BoundingBox.Top - BoundingBox.Bottom) * marginFactor;
            var left = BoundingBox.Left - marginX;
            var bottom = BoundingBox.Bottom - marginY;
            var top = BoundingBox.Top + marginY;
            var right = BoundingBox.Right + marginX;
            MultiPoint lr = new MultiPoint(new[]
            {
                new Coordinate(left, bottom),
                new Coordinate(left, top),
                new Coordinate(right, top),
                new Coordinate(right, bottom),
                new Coordinate(left, bottom),
            });
            return lr.EnvelopeAsGeometry;
        }

        public readonly List<OmrådeMedGeometry> Områder = new List<OmrådeMedGeometry>();

        public VectorQuadTile(TileCoordinates tile, TiledVectorLayer layer) : base(tile.GetExtent(layer), tile.ZoomLevel, tile.X, tile.Y)
        {
        }
    }

    [Serializable]
    public class OmrådeMedGeometry
    {
        public Geometri.Område Område;
        public Geometry Geometry;

        public OmrådeMedGeometry(Geometri.Område område, Geometry geometry)
        {
            Geometry = geometry;
            Område = område;
        }
    }
}