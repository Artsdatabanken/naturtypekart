using System;
using DotSpatial.Topology;

namespace Nin.Common.Map.Geometric.Grids
{
    /// <summary>
    /// SSBgrid is an open-ended definition of a family of spatial tessellation models for use in Norway. The 
    /// models are all built with quadratic grid cells.The naming convention of the grids is to use the grid cell 
    /// size, defined as the length of a side of a grid cell, and the unit of measurement (KM for kilometres and 
    /// M for meters) concatenated to the capital letters ‘SSB’. As an example, SSB1KM will be the name of 
    /// the SSBgrid composed of grid cells with size 1 X 1 kilometer(1 km2 ). Any quadratic grid size is
    /// possible.For grids covering only parts of Norway, a reference to the administrative or other kind of 
    /// unit can be added to the name.
    /// https://www.ssb.no/a/english/publikasjoner/pdf/doc_200909_en/doc_200909_en.pdf
    /// </summary>
    public class SsbGrid
    {
        private readonly double gridSizeMeters;

        public SsbGrid(int gridSizeMeters)
        {
            this.gridSizeMeters = gridSizeMeters;
        }

        public Coordinate[] GetPolygon(long cellId)
        {
            var ll = GetGridCellLowerLeftCoordinate(cellId);
            return CreatePolygonFromLowerLeftCoordinate(ll);
        }

        public Coordinate[] GetPolygon(Coordinate lowerLeftCoordinate)
        {
            return CreatePolygonFromLowerLeftCoordinate(lowerLeftCoordinate);
        }

        private Coordinate[] CreatePolygonFromLowerLeftCoordinate(Coordinate ll)
        {
            return new[]
            {
                new Coordinate(ll.X, ll.Y),
                new Coordinate(ll.X + gridSizeMeters, ll.Y),
                new Coordinate(ll.X + gridSizeMeters, ll.Y + gridSizeMeters),
                new Coordinate(ll.X, ll.Y + gridSizeMeters),
                new Coordinate(ll.X, ll.Y),
            };
        }

        public long GetCellId(Coordinate coordinate)
        {
            return GetCellId(coordinate.X, coordinate.Y);
        }

        public long GetCellId(double x, double y)
        {
            long id = (long)(Math.Truncate((x + 2e6) / gridSizeMeters) * gridSizeMeters * 1e7) + 
                (long)(Math.Truncate(y / gridSizeMeters) * gridSizeMeters);
            return id;
        }

        public Coordinate GetGridCellLowerLeftCoordinate(long id)
        {
            var truncate = Math.Truncate(id * 1e-7);
            double x = truncate - 2e6 + gridSizeMeters / 2.0;
            double y = id - truncate * 1e7;
            return new Coordinate(x, y);
        }

        public Coordinate GetLowerLeft(int x, int y)
        {
            long id = GetCellId(x, y);
            return GetGridCellLowerLeftCoordinate(id);
        }

        public Coordinate GetUpperRight(int x, int y)
        {
            long id = GetCellId(x, y);
            var c = GetGridCellLowerLeftCoordinate(id);
            c.X += gridSizeMeters;
            c.Y += gridSizeMeters;
            return c;
        }
    }
}