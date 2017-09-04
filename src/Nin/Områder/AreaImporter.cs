using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nin.Områder
{
    public static class AreaImporter
    {
        public static void BulkStore(IEnumerable<Area> areas)
        {
            IO.SqlServer.SqlServer.BulkStoreAreas(areas);
        }

        public static void Store(IEnumerable<Area> areas)
        {
            IO.SqlServer.SqlServer.StoreAreas(areas);
        }

        public static void DeleteAreas(AreaType areaType)
        {
            IO.SqlServer.SqlServer.DeleteAreas(areaType);
        }

        public static void UpdateAreas(Collection<Area> areas)
        {
            IO.SqlServer.SqlServer.UpdateAreas(areas);
        }
    }
}