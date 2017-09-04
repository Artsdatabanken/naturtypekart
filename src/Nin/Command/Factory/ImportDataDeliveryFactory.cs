using System.Collections.Generic;
using Common;

namespace Nin.Command.Factory
{
    class ImportDataDeliveryFactory : FactoryBase
    {
        public override DatabaseCommand Create(CommandLineArguments args)
        {
            return new ImportDataDeliveryCommand(
                args.DeQueue());
        }

        protected override IEnumerable<string> GetVerbs() { return new[] { "importdatadelivery", "idd" }; }
        public override string Usage => "importdatadelivery <FileSpec.xml>\r\n   Imports the specified datadelivery xml file (NIN).";
    }
}