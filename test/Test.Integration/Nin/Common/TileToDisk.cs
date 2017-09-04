using System;
using System.Collections.Generic;
using System.IO;
using Common;
using DotSpatial.Topology;
using Newtonsoft.Json;
using Nin.Common.Map.Tiles;
using Nin.Configuration;
using Nin.Map.Layers;
using Nin.Map.Tiles;
using Nin.Map.Tiles.Geometri;
using Nin.Map.Tiles.Stores;
using Nin.Map.Tiles.Vectors;
using Nin.Områder;
using NUnit.Framework;
using FeatureCollection = GeoJSON.Net.Feature.FeatureCollection;
using Polygon = DotSpatial.Topology.Polygon;

namespace Test.Integration.Nin.Common
{
    public class TileToDisk
    {
        private readonly VectorTiler tiler;

        [Test]
        public void TileNorway()
        {
            var filename = FileLocator.FindFileInTree(@"data\norway.geojson");
            FeatureCollection norway = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText(filename));
            List<OmrådeMedGeometry> partOfNorway = new List<OmrådeMedGeometry>();
            foreach (var feature in norway.Features)
                partOfNorway.Add(new OmrådeMedGeometry(new Område(3581, AreaType.Land), DotSpatialGeometry.From(feature)));
            foreach (var omg in partOfNorway)
                omg.Område.Number = -5;
            for (int zoom = 0; zoom < 6; zoom++)
                foreach (OmrådeMedGeometry polygon in partOfNorway)
                    tiler.Update(polygon.Område, polygon.Geometry, zoom);
        }

        [Test]
        public void TileGrid()
        {
            for (int zoom = 0; zoom < 5; zoom++)
            {
                IEnumerable<OmrådeMedGeometry> grid = GreateGrid(zoom);
                foreach (var e in grid)
                    tiler.Update(e.Område, e.Geometry, zoom);
            }
        }

        private static IEnumerable<OmrådeMedGeometry> GreateGrid(int zoom)
        {
            List<OmrådeMedGeometry> p = new List<OmrådeMedGeometry>();
            var bb = WebMercator.BoundingBox;
            int max = 2 << (zoom + 3);
            var rnd = new Random();
            for (int i = 0; i < 2; i++)
            {
                var Width = bb.Maximum.X - bb.Minimum.X;
                var Height = bb.Maximum.Y - bb.Minimum.Y;
                double x1 = rnd.NextDouble() * Width + bb.Minimum.X;
                double y1 = rnd.NextDouble() * Height + bb.Minimum.Y;
                double x2 = rnd.NextDouble() * Width + bb.Minimum.X;
                double y2 = rnd.NextDouble() * Height + bb.Minimum.Y;
                double x3 = rnd.NextDouble() * Width + bb.Minimum.X;
                double y3 = rnd.NextDouble() * Height + bb.Minimum.Y;
                var polygon = new List<Coordinate>();
                polygon.AddRange(new[] { new Coordinate(x1, y1), new Coordinate(x2, y2), new Coordinate(x3, y3), new Coordinate(x1, y1) });
                var poly = new Polygon(polygon);
                var o = new Område(-1, AreaType.Grid) {Number = 1};
                p.Add(new OmrådeMedGeometry(o, poly));
            }

            double left = (bb.Maximum.X - bb.Minimum.X) / 40 + bb.Minimum.X;
            double right = bb.Maximum.X - (bb.Maximum.X - bb.Minimum.X) / 40;
            double top = bb.Maximum.Y - (bb.Maximum.Y - bb.Minimum.Y) / 40;
            double bottom = bb.Minimum.Y + (bb.Maximum.Y - bb.Minimum.Y) / 40;
            var dy = (top - bottom) * max / 500.0;
            //for (double x = bb.Left; x <= bb.Right; x += dx)
            //    p.Add(new Polygon(new[] { new Point(x, bb.Bottom), new Point(bb.Right - x, bb.Top), new Point(0, 0), new Point(x, bb.Bottom) }));
            for (double y = bottom; y <= top; y += dy)
            {
                var polygon = new List<Coordinate>();
                polygon.AddRange(new[] {
                    new Coordinate(left+1, y),
                    new Coordinate(right-1, y),
                    new Coordinate(right-1, y+dy/2-1),
                    new Coordinate(left+1, y+dy/2-1),
                    new Coordinate(left+1, y)});
                var o1 = new Område(-1, AreaType.Grid) {Number = 1};
                p.Add(new OmrådeMedGeometry(o1, new LineString(polygon)));
            }

            var p2 = new List<Coordinate>();
            p2.AddRange(new[] {
                new Coordinate(bb.Minimum.X, bb.Minimum.Y), new Coordinate(bb.Maximum.X, bb.Minimum.Y),
                new Coordinate(bb.Maximum.X, bb.Maximum.Y), new Coordinate(bb.Minimum.X, bb.Maximum.Y), new Coordinate(bb.Minimum.X, bb.Minimum.Y)
            });
            var område = new Område(-1, AreaType.Grid) {Number = 1};
            p.Add(new OmrådeMedGeometry(område, new LineString(p2)));

            return p;
        }

        public TileToDisk()
        {
            var layer = new TiledVectorLayer("test2", WebMercator.BoundingBox1, 39135.75848201024 * 4);
            var store = new DiskTileStore(layer);
            store.Wipe();
            tiler = new VectorTiler(store, layer);
        }
    }
}