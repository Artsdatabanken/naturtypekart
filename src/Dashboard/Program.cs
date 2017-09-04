using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Nin.Common.Map.Tiles;
using Nin.Configuration;
using Nin.Dataleveranser;
using Nin.IO.RavenDb;
using Nin.Map.Layers;
using Nin.Naturtyper;
using Nin.Session;
using Nin.Tasks;

namespace Dashboard
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //new TestSetup().RunBeforeAnyTests();
            //new TileMapTaskTest().TileGridTask();
            //var ddv = new DataDeliveryValidatorTest();
            //ddv.ValidateAdminAreaMapImport_ValidationOk();
            //ddv.ValidateAreaMapImport_ValidationOk();
            //ddv.ValidateDataDeliveryExportTest_ValidationOk();
            //ddv.ValidateDataDeliveryTest_ValidationOk();
            //ddv.ValidateGridMapImport_ValidationOk();
            InitializeDefaultConfiguration();
            PubliserDataleveranse("833");
            LastOppDataleveranse("e:\\naturtypekart\\schema\\mdir_alle102.xml");
            Config.Settings.SaveToDirectory(".");
            HentKode("NA_T34-C-2");
            ExportAsGml();
            ProcessQueue();
        }

        private static void LastOppDataleveranse(string xmlPath)
        {
            var arkiv = new NinRavenDb();
            string fileContent;
            using (var streamReader = new StreamReader(xmlPath, Encoding.GetEncoding("iso-8859-1")))
                fileContent = streamReader.ReadToEnd();
            XDocument dataDeliveryXml = XDocument.Parse(fileContent);
            var dataleveranse = DataleveranseXmlGreier.ParseDataDelivery(dataDeliveryXml);
//            arkiv.LastOppDataleveranse(new DataleveranseXmlGreier(), metadata, files, username);
        }

        private static void PubliserDataleveranse(string id)
        {
            var arkiv = new NinRavenDb();
            var userDb = new ArtsdatabankenUserDatabase();
            DataleveransePubliserer.PubliserLeveranse(id, arkiv, userDb );
        }

        private static void HentKode(string kode)
        {
            var ki = Naturkodetrær.Naturtyper.HentFraKode(kode);
            Console.WriteLine(ki);
        }

        private static void ExportAsGml()
        {
            
        }

        private static void InitializeDefaultConfiguration()
        {
            Config.Settings = Config.LoadFromExecutablePath();
            Config.Settings.Map.Layers.Clear();
            Config.Settings.Map.Layers.Add(new TiledVectorLayer("Nin", WebMercator.BoundingBox1, 39135.75848201024 * 4));
        }

        private static void ProcessQueue()
        {
            Config.Settings.ConnectionString = Environment.GetEnvironmentVariable("DB") ?? Config.Settings.ConnectionString;
            Config.Settings.ConnectionString = "Data Source=;Initial Catalog=;User=;Password=";
            //Config.Settings.ConnectionString = "Data Source=localhost;Initial Catalog=ninboci;integrated security=true";
            var taskQueue = new TaskQueue();
            TaskQueue.Wipe();
            taskQueue.Enqueue("TileArea", "{\"AreaType\":1,\"Number\":1841, \"MapLayerName\":\"Nin\"}");
            var context = new NinServiceContext();
            TaskQueue.ProcessNext(context);
        }
    }
}
