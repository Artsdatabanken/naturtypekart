using System.Collections.Generic;
using Common;

namespace Nin.Command.Factory
{
    class CreateDatabaseFactory : FactoryBase
    {
        public override DatabaseCommand Create(CommandLineArguments args)
        {
            return new CreateDatabaseCommand(args.DeQueue());
        }

        protected override IEnumerable<string> GetVerbs() { return new[] { "createdb", "createdatabase" }; }
        public override string Usage => "createdb <SqlSchemaScriptFileSpec>\r\n   Drops database if it exists, create a new database and runs scripts.";
    }
}