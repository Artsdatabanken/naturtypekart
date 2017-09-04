using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common;

namespace Nin.Command.Factory
{
    public class CommandFactory
    {
        readonly List<FactoryBase> availableCommands = new List<FactoryBase>();

        public DatabaseCommand Parse(IEnumerable<string> args)
        {
            CommandLineArguments cla = new CommandLineArguments(args, Usage());
            string commandName = cla.DeQueue();
            foreach (FactoryBase candidate in availableCommands)
            {
                if (!candidate.Accepts(commandName)) continue;

                return candidate.Create(cla);
            }
            throw Usage();
        }

        public CommandFactory()
        {
            availableCommands.Add(new CreateDatabaseFactory());
            availableCommands.Add(new UpgradeFactory());
            availableCommands.Add(new ExecuteSqlFactory());
            availableCommands.Add(new SysLogFactory());
            availableCommands.Add(new ImportAreaFactory());
            availableCommands.Add(new ImporterRutenettFactory());
            availableCommands.Add(new ImportAreaLayerFactory());
            availableCommands.Add(new ImportDataDeliveryFactory());
        }

        private Exception Usage()
        {
            string exeName = Process.GetCurrentProcess().ProcessName;
            string usage = "\r\nUSAGE: \r\n";
            foreach (var factory in availableCommands)
                usage += $"\r\n  {exeName} {factory.Usage}\r\n";
            usage += Environment.NewLine;
            return new Exception(usage);
        }
    }
}