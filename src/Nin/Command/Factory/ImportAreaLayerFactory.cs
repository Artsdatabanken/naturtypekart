using System.Collections.Generic;
using Common;

namespace Nin.Command.Factory
{
    class ImportAreaLayerFactory : FactoryBase
    {
        public override DatabaseCommand Create(CommandLineArguments args)
        {
            return new ImportAreaLayerCommand(args.DeQueue());
        }

        protected override IEnumerable<string> GetVerbs() { return new[] { "importarealayer", "ial" }; }
        public override string Usage => "importarealayer <AreaFileSpec.xml>\r\n   Imports the specified area layer (values) data file (xml).";
    }
}