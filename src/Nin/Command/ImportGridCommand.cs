using Nin.Dataleveranser.Rutenett;
using Nin.Diagnostic;
using Nin.Rutenett;

namespace Nin.Command
{
    public class ImportGridCommand : DatabaseCommand
    {
        public override void Execute()
        {
            var grid = GridImporter.ImportGrid(sourceEpsgCode, rutenettType, dataFile);
            GridImporter.BulkStore(grid);
            Log.i("DB", $"Imported {grid.Cells.Count} cells from {dataFile}.");
        }

        public ImportGridCommand(string dataFile, int sourceEpsgCode, RutenettType rutenettType)
        {
            this.rutenettType = rutenettType;
            this.sourceEpsgCode = sourceEpsgCode;
            this.dataFile = dataFile;
        }

        protected override string GetDescription()
        {
            return $"Imports grid {rutenettType} dataset from file \'{dataFile}\' (SRID {sourceEpsgCode}).";
        }

        private readonly string dataFile;
        private readonly int sourceEpsgCode;
        private readonly RutenettType rutenettType;
    }
}