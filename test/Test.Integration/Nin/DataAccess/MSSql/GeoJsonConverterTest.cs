using System.Collections.ObjectModel;
using Nin.Dataleveranser.Rutenett;
using Nin.GeoJson;
using Nin.IO.SqlServer;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess.MSSql
{
    public class GeoJsonConverterTest
    {
        [Test]
        public void Grid500KmToGeoJsonTest()
        {
            new GridImportTest().StoreSsb500KmTest();

            var grid = SqlServer.GetGrid(RutenettType.SSB500KM, new Collection<int>(), new Collection<int>(), "", "", 3857, 0);
            var gridJson = GeoJsonConverter.GridToGeoJson(grid, false);
            Assert.True(gridJson.Length > 10000); // 10479
        }

        [Test][Ignore("Runtime > 1 minute")]
        public void Grid1KmToGeoJsonTest()
        {
            new GridImportTest().StoreSsb001KmTest();

            var grid = SqlServer.GetGrid(RutenettType.SSB001KM, new Collection<int>(), new Collection<int>(), "", "", 3857, 0);
            var gridJson = GeoJsonConverter.GridToGeoJson(grid, false);
            Assert.IsNotEmpty(gridJson);
            Assert.AreEqual(152909657, gridJson.Length);
        }
    }
}
