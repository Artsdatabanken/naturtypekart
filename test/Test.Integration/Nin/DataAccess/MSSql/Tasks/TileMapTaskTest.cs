using Nin.Configuration;
using Nin.Map.Tiles.Stores;
using Nin.Områder;
using Nin.Tasks;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess.MSSql.Tasks
{
    public class TileMapTaskTest
    {
        [Test]
        public void TileGridTask()
        {
            new DiskTileStore(Config.Settings.Map.Layers[0]).Wipe();
            TestTaskQueue.ProcessTask(new TileAreaTask(AreaType.Kommune, 0, "Nin"));
        }
    }
}