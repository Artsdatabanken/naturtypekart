using System.Data.SqlTypes;
using Common.Rutenett;
using DotSpatial.Data;
using Microsoft.SqlServer.Types;
using Nin.Dataleveranser.Rutenett;
using Nin.Types.GridTypes;

namespace Nin.Common.Map.Geometric.Grids
{
    public static class Grid2
    {
        public static Grid FromShapeFile(string shapeFilePath, RutenettType rutenettType, int epsgCode)
        {
            var grid = new Grid(rutenettType);

            Shapefile shapeFile = Shapefile.OpenFile(shapeFilePath);
            foreach (var feature in shapeFile.Features)
            {
                var cell = new GridCell();
                cell.CellId = feature.DataRow[0].ToString();
                var text = new SqlChars(feature.BasicGeometry.ToString());
                cell.Geometry = SqlGeometry.STGeomFromText(text, epsgCode);
                grid.Cells.Add(cell);
            }

            return grid;
        }
    }
}
