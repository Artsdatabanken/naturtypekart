using System.Collections.Generic;
using Common;
namespace Nin.Command.Factory
{
    class ExecuteSqlFactory : FactoryBase
    {
        public override DatabaseCommand Create(CommandLineArguments args)
        {
            return new ExecuteSqlCommand(args.DeQueue());
        }

        protected override IEnumerable<string> GetVerbs() { return new[] { "exec" }; }
        public override string Usage => "exec <SqlQuery>\r\n   Executes the specified SQL statements.";
    }
}