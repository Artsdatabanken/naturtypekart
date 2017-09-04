using System.Collections.Generic;
using Common;
using Nin.Dataleveranser.Rutenett;

namespace Nin.Command.Factory
{
    class ImporterRutenettFactory : FactoryBase
    {
        public override DatabaseCommand Create(CommandLineArguments args)
        {
            return new ImportGridCommand(args.DeQueue(), args.DequeInt("epsgCode"), args.DeQueueEnum<RutenettType>("rutenettType"));
        }

        protected override IEnumerable<string> GetVerbs() { return new[] { "importgrid", "ig" }; }
        public override string Usage => "importgrid <GridFileSpec> <EpsgCode> <rutenettType>\r\n   Imports the specified grid data file.";
    }
}