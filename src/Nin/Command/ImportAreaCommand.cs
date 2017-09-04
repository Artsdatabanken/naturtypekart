using Nin.Diagnostic;
using Nin.Områder;

namespace Nin.Command
{
    public class ImportAreaCommand : DatabaseCommand
    {
        public override void Execute()
        {
            var areas = AreaCollection.ImportAreasFromFile(dataFile, sourceSrid, areaType);
            AreaImporter.BulkStore(areas);
            Log.i("DB", $"Imported {areas.Count} areas of type {areaType}.");
        }

        public ImportAreaCommand(string dataFile, int sourceSrid, AreaType areaType)
        {
            this.areaType = areaType;
            this.dataFile = dataFile;
            this.sourceSrid = sourceSrid;
        }

        protected override string GetDescription()
        {
            return $"Importing {areaType} from file \'{dataFile} (SRID {sourceSrid})\'.";
        }

        private readonly string dataFile;
        private readonly int sourceSrid;
        private readonly AreaType areaType;
    }
}