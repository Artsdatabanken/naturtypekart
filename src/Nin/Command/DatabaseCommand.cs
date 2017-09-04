using System.Collections.Generic;
using Nin.Command.Factory;

namespace Nin.Command
{
    public abstract class DatabaseCommand
    {
        public abstract void Execute();

        public static DatabaseCommand Parse(IEnumerable<string> args)
        {
            return new CommandFactory().Parse(args);
        }
        
        public string Description()
        {
            string sub = GetDescription();
            //var cs = new SqlConnectionStringBuilder(Config.Settings.ConnectionString);
            return sub;
                //$"{sub}\r\nConnecting to server '{cs.DataSource}', schema '{cs.InitialCatalog}'.";
        }
        protected abstract string GetDescription();
    }
}