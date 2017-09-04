using System.Collections.Generic;
using Common;
using Nin.Områder;

namespace Nin.Command.Factory
{
    class ImportAreaFactory : FactoryBase
    {
        public override DatabaseCommand Create(CommandLineArguments args)
        {
            return new ImportAreaCommand(
                args.DeQueue(), 
                args.DeQueueInt("SourceSrid", 4326), 
                args.DeQueueEnum<AreaType>("områdeType"));
        }

        protected override IEnumerable<string> GetVerbs() { return new[] { "importarea", "ia" }; }
        public override string Usage => "importarea <AreaFileSpec> <SourceSrid> <OmrådeType>\r\n   Imports the specified area data file (geojson/shp).";
    }
}