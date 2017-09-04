using System;
using System.Collections.Generic;
using Common;
using Nin.Common;

namespace Nin.Command.Factory
{
    class SysLogFactory : FactoryBase
    {
        public override DatabaseCommand Create(CommandLineArguments args)
        {
            LogPriority logPriority = LogPriority.Verbose;
            if(args.Count > 0)
                logPriority = Map(args.DeQueue());
            return new SysLogCommand(logPriority);
        }

        private static LogPriority Map(string priority)
        {
            return (LogPriority)Enum.Parse(typeof(LogPriority), priority, true);
        }

        protected override IEnumerable<string> GetVerbs() { return new[] { "syslog", "sl", "log" }; }
        public override string Usage => "syslog <Verbose/Debug/Info/Warn/Error/Assert>\r\n   Display all syslog events or events having at least minimum priority.";
    }
}