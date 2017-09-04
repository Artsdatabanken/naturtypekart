using System;
using System.Collections.ObjectModel;
using System.IO;
using Nin.Configuration;
using Nin.Dataleveranser.Rutenett;
using Nin.IO.SqlServer;
using Nin.Rutenett;
using Nin.Types.GridTypes;
using Nin.Types.MsSql;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess.MSSql
{
    class GridImportTest
    {
        [Test][Ignore("Times out (SQL connection timeout) after 22 minutes run time on test.")]
        public void StoreSsb001KmTest()
        {
            ImportGrid(@"ssb1km.shp", 32633, RutenettType.SSB001KM);
        }

        [Test]
        public void StoreSsb005KmTest()
        {
            ImportGrid(@"ssb5km.shp", 32633, RutenettType.SSB005KM);
        }

        [Test]
        public void StoreSsb010KmTest()
        {
            ImportGrid(@"ssb10km.shp", 32633, RutenettType.SSB010KM);
        }

        [Test]
        public void StoreSsb500KmTest()
        {
            ImportGrid(@"ssb500km.shp", 32633, RutenettType.SSB500KM);
        }

        [Test]
        public void GetGridTest()
        {
            SqlServer.GetGrid(RutenettType.SSB010KM, new Collection<int> { 1576 }, new Collection<int> { 15, 6 }, "", "", 0, 0);
        }

        [Test]
        public void StoreGridLayerTest()
        {
            var gridLayer = new GridLayer("Kalklag test", RutenettType.SSB010KM)
            {
                Description = "Test av kalklag",
                Code = new Code {Value = "KA", Registry = "NiN", Version = "2.0"},
                Owner = new Contact {Company = "Artsdatabanken"},
                Established = DateTime.Now,
                MinValue = "0.0",
                MaxValue = "3.0"
            };

            gridLayer.Cells.Add(new GridLayerCell { CellId = "22600007060000", Value = "0.0" });
            gridLayer.Cells.Add(new GridLayerCell { CellId = "22700007060000", Value = "1.0" });
            gridLayer.Cells.Add(new GridLayerCell { CellId = "22600007070000", Value = "2.0" });
            gridLayer.Cells.Add(new GridLayerCell { CellId = "22700007070000", Value = "3.0" });

            SqlServer.BulkStoreGridLayer(gridLayer);
        }

        [Test]
        public void GetGridLayerTest()
        {
            SqlServer.GetGrid(RutenettType.SSB010KM, new Collection<int>(), new Collection<int>(), "", "", 0, 1);
        }

        [Test]
        public void GetGridSummaryTest()
        {
            SqlServer.GetGridSummary();
        }

        [Test]
        public void GetAreaLayerSummaryTest()
        {
            SqlServer.GetAreaLayerSummary();
        }

        [Test]
        public void StoreGridLayerTest2()
        {
            var gl = new GridLayer("Kalklag test 2", RutenettType.SSB010KM)
            {
                Description = "Test av kalklag nummer 2",
                Code = new Code {Value = "KA", Registry = "NiN", Version = "2.0"},
                Owner = new Contact {Company = "Artsdatabanken"},
                Established = DateTime.Now,
                MinValue = "1",
                MaxValue = "9"
            };

            var cells = gl.Cells;
            cells.Add(new GridLayerCell { CellId = "20600006440000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "20200006450000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "20300006450000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "20400006450000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20500006450000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "20600006450000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20700006450000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "20800006450000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "20900006450000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "20000006460000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "20100006460000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "20200006460000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "20300006460000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20400006460000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "20500006460000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20600006460000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "20700006460000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "20800006460000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "20900006460000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "21000006460000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "20000006470000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "20100006470000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20200006470000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "20300006470000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20400006470000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "20500006470000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "20600006470000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "20700006470000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "20800006470000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "20900006470000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "21000006470000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "21100006470000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "21200006470000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20000006480000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "20100006480000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "20200006480000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "20300006480000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "20400006480000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "20500006480000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "20600006480000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20700006480000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "20800006480000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20900006480000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "21000006480000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "21100006480000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "21200006480000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "21300006480000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "19800006490000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "19900006490000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20000006490000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "20100006490000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20200006490000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "20300006490000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "20400006490000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "20500006490000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "20600006490000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "20700006490000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "20800006490000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20900006490000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "21000006490000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "21100006490000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "21200006490000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "21300006490000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "21400006490000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "19700006500000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "19800006500000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "19900006500000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20000006500000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "20100006500000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20200006500000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "20300006500000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "20400006500000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "20500006500000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "20600006500000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "20700006500000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "20800006500000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20900006500000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "21000006500000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "21100006500000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "21200006500000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "21300006500000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "21400006500000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "21500006500000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "19600006510000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "19700006510000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "19800006510000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "19900006510000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20000006510000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "20100006510000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "20200006510000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "20300006510000", Value = "1" });
            cells.Add(new GridLayerCell { CellId = "20400006510000", Value = "2" });
            cells.Add(new GridLayerCell { CellId = "20500006510000", Value = "3" });
            cells.Add(new GridLayerCell { CellId = "20600006510000", Value = "4" });
            cells.Add(new GridLayerCell { CellId = "20700006510000", Value = "5" });
            cells.Add(new GridLayerCell { CellId = "20800006510000", Value = "6" });
            cells.Add(new GridLayerCell { CellId = "20900006510000", Value = "7" });
            cells.Add(new GridLayerCell { CellId = "21000006510000", Value = "8" });
            cells.Add(new GridLayerCell { CellId = "21100006510000", Value = "9" });
            cells.Add(new GridLayerCell { CellId = "21200006510000", Value = "1" });

            SqlServer.BulkStoreGridLayer(gl);
        }

        private static void ImportGrid(string shapeFilePath, int sourceEpsgCode, RutenettType rutenettType)
        {
            string fullPath = FileLocator.FindFileInTree(Path.Combine(@"Data\Grid\SSB", shapeFilePath));

            var grid = GridImporter.ImportGrid(sourceEpsgCode, rutenettType, fullPath);
            GridImporter.BulkStore(grid);
        }
    }
}
