using System.Collections.Generic;
using Common;

namespace Nin.Command.Factory
{
    class UpgradeFactory : FactoryBase
    {
        public override DatabaseCommand Create(CommandLineArguments args)
        {
            return new UpgradeCommand(args.DeQueue());
        }

        protected override IEnumerable<string> GetVerbs() { return new[] { "upgrade" }; }
        public override string Usage => "upgrade <SqlSchemaScriptFileSpec>\r\n   Runs the scripts specified that have not yet been run on the database.";
    }
}