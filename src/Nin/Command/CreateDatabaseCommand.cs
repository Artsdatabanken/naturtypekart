using Nin.Diagnostic;
using Nin.IO.SqlServer;

namespace Nin.Command
{
    public class CreateDatabaseCommand : DatabaseCommand
    {
        public override void Execute()
        {
            Log.Suspend();
            SqlServerDatabase.WipeAndCreate(scriptDirectory);
            Log.Resume();
            Log.i("DB", "Database created successfully.");
        }

        public CreateDatabaseCommand(string scriptDirectory)
        {
            this.scriptDirectory = scriptDirectory;
        }

        protected override string GetDescription()
        {
            return "Creating database from scratch using script files '" + scriptDirectory + "'.";
        }

        readonly string scriptDirectory;
    }
}
