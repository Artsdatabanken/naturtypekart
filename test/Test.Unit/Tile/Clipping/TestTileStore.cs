using System.Collections.Generic;
using System.Text;
using Nin.Common.Map.Tiles.Stores;
using Nin.Map.Layers;
using Nin.Map.Tiles;
using Nin.Map.Tiles.Vectors;

namespace Test.Unit.Tile.Clipping
{
    public class TestTileStore : IPersistStuff<VectorQuadTile>
    {
        readonly List<VectorQuadTile> updates = new List<VectorQuadTile>();
        private readonly TiledVectorLayer layer;

        public TestTileStore(TiledVectorLayer layer)
        {
            this.layer = layer;
        }

        private IEnumerable<VectorQuadTile> Updates => updates;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (VectorQuadTile tile in Updates)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append("{" + tile.TileCoordinates + ": ");
                bool first = true;
                foreach (var polygon in tile.Områder)
                {
                    if (!first) sb.Append(", ");
                    first = false;
                    sb.AppendFormat(polygon.Geometry.ToString());
                }
            }
            return "[" + sb + "]";
        }

        public void Save(string key, VectorQuadTile tile)
        {
            updates.Add(tile);
        }

        public VectorQuadTile Load(string key)
        {
            return new VectorQuadTile(TileCoordinates.FromRelativePath(key), layer);
        }
    }
}