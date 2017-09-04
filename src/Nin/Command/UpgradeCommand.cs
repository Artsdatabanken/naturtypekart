using Nin.Diagnostic;
using Nin.IO.SqlServer;

namespace Nin.Command
{
    public class UpgradeCommand : DatabaseCommand
    {
        public override void Execute()
        {
            SqlServerDatabase.UpgradeExisting(scriptDirectory);
            Log.i("DB", "Database upgraded successfully.");
        }

        public UpgradeCommand(string scriptDirectory)
        {
            this.scriptDirectory = scriptDirectory;
        }

        protected override string GetDescription()
        {
            return "Updating database by runnning only scripts not alreay in database from '" + scriptDirectory + "'.";
        }

        readonly string scriptDirectory;
    }
}