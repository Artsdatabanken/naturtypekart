using Nin.Diagnostic;
using Nin.Områder;
using SqlServer = Nin.IO.SqlServer.SqlServer;

namespace Nin.Command
{
    public class ImportAreaLayerCommand : DatabaseCommand
    {
        public override void Execute()
        {
            var layer = AreaLayerImpl.FraAdministrativtOmrådeNinXml(dataFile);
            int count = SqlServer.BulkStoreAreaLayer(layer);
            Log.i("DB", $"Imported {count} items from {dataFile}.");
        }

        public ImportAreaLayerCommand(string dataFile)
        {
            this.dataFile = dataFile;
        }

        protected override string GetDescription()
        {
            return "Importing area layer values from file '" + dataFile + "'.";
        }

        private readonly string dataFile;
    }
}