using Common.Rutenett;
using Nin.Common.Map.Geometric.Grids;
using Nin.Configuration;
using Nin.Dataleveranser.Rutenett;
using Raven.Abstractions.Extensions;

namespace Nin.Rutenett
{
    public class GridImporter
    {
        public static Grid ImportGrid(int sourceEpsgCode, RutenettType rutenettType, string fullPath)
        {
            var grid = Grid2.FromShapeFile(fullPath, rutenettType, sourceEpsgCode);
            Grid importGrid = new Grid(rutenettType);
            if (sourceEpsgCode != Config.Settings.Map.SpatialReferenceSystemIdentifier)
            {
                MapProjection reproject = new MapProjection(Config.Settings.Map.SpatialReferenceSystemIdentifier);

                foreach (var cell in grid.Cells)
                {
                    if (!reproject.IsInsideBounds(cell.Geometry)) continue; // TODO: What do we do?

                    cell.Geometry = reproject.Reproject(cell.Geometry);

                    importGrid.Cells.Add(cell);
                }
            }

            else importGrid.Cells.AddRange(grid.Cells);

            return importGrid;
        }

        public static void BulkStore(Grid grid)
        {
            IO.SqlServer.SqlServer.BulkStoreGrid(grid);
        }
    }
}