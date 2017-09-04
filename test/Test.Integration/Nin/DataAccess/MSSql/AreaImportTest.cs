
using System;
using System.IO;
using System.Text;
using Nin;
using Nin.Configuration;
using Nin.IO.SqlServer;
using Nin.Områder;
using Nin.Types.MsSql;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess.MSSql
{
    class AreaImportTest
    {
        readonly string dataFylkerGeojson = FileLocator.FindFileInTree(@"Data\Area\fylker_subset.geojson");
        readonly string dataKommunerGeojson = FileLocator.FindFileInTree(@"Data\Area\kommuner_subset.geojson");
        readonly string naturvernomraderUtm33Shp = FileLocator.FindFileInTree(@"Data\Area\Naturvernområder_utm33_subset\naturvernomrader_utm33.shp");

        [Test]
        public void StoreCountiesTest()
        {
            var areas = AreaCollection.ImportAreasFromFile(dataFylkerGeojson, 4326);
            AreaImporter.Store(areas);
        }

        [Test]
        public void BulkStoreCountiesTest()
        {
            var areas = AreaCollection.ImportAreasFromFile(dataFylkerGeojson, 4326);
            AreaImporter.BulkStore(areas);
        }

        [Test]
        public void StoreMunicipalitiesTest()
        {
            var areas = AreaCollection.ImportAreasFromFile(dataKommunerGeojson, 4326);
            AreaImporter.Store(areas);
        }

        [Test]
        public void BulkStoreMunicipalitiesTest()
        {
            var areas = AreaCollection.ImportAreasFromFile(dataKommunerGeojson, 4326);
            AreaImporter.BulkStore(areas);
        }

        // Verneområder: http://kartkatalog.miljodirektoratet.no/map_catalog_dataset.asp?datasetid=0&download=yes
        [Test]
        public void BulkStoreConservationAreasTest()
        {
            var conservationAreas = AreaCollection.ImportAreasFromFile(
                naturvernomraderUtm33Shp, 32633, AreaType.Verneområde);
            AreaImporter.BulkStore(conservationAreas);
        }

        [Test]
        public void DeleteMunicipalitiesTest()
        {
            AreaImporter.DeleteAreas(AreaType.Kommune);
        }

        [Test]
        public void DeleteCountiesTest()
        {
            AreaImporter.DeleteAreas(AreaType.Fylke);
        }

        [Test][Ignore("Population is only used for testing during development.")]
        public void BulkStoreCountyPopulation()
        {
            AreaLayer countyPopulation =
                new AreaLayer("Befolkning", AreaType.Fylke)
                {
                    Description = "Befolkning per fylke",
                    Code = new Code {Value = "KA", Registry = "NiN", Version = "2.0"},
                    Owner = new Contact {Company = "Artsdatabanken"},
                    Established = DateTime.Now
                };

            using (
                StreamReader countyPopulationFile =
                    new StreamReader(@"C:\Artsdatabanken\SSB\Befolkning\BefolkningFylker.csv",
                        Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = countyPopulationFile.ReadLine()) != null)
                {
                    int number = int.Parse(line.Substring(0, 2));
                    string population = line.Substring(line.IndexOf(';') + 1);

                    countyPopulation.Items.Add(new AreaLayerItem { Number = number, Value = population });
                }
            }
            countyPopulation.MinValue = "70000";
            countyPopulation.MaxValue = "650000";

            SqlServer.BulkStoreAreaLayer(countyPopulation);
        }

        [Test][Ignore("Population is only used for testing during development.")]
        public void BulkStoreMunicipalityPopulation()
        {
            var befolkningKommune =
                new AreaLayer("Befolkning", AreaType.Kommune)
                {
                    Description = "Befolkning per kommune",
                    Code = new Code {Value = "KA", Registry = "NiN", Version = "2.0"},
                    Owner = new Contact {Company = "Artsdatabanken"},
                    Established = DateTime.Now
                };

            using (StreamReader municipalityPopulationFile = new StreamReader(@"C:\Artsdatabanken\SSB\Befolkning\BefolkningKommuner.csv", 
                Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = municipalityPopulationFile.ReadLine()) != null)
                {
                    var number = int.Parse(line.Substring(0, 4));
                    string population = line.Substring(line.IndexOf(';') + 1);

                    befolkningKommune.Items.Add(new AreaLayerItem { Number = number, Value = population });
                }
            }
            befolkningKommune.MinValue = "500";
            befolkningKommune.MaxValue = "100000";

            SqlServer.BulkStoreAreaLayer(befolkningKommune);
        }

        [Test]
        public void UpdateCountiesTest()
        {
            var areas = AreaCollection.ImportAreasFromFile(dataFylkerGeojson, 4326);
            AreaImporter.UpdateAreas(areas);
        }
    }
}