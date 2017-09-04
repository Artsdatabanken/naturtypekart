using System.Collections.Generic;
using Common;

namespace Nin.Command.Factory
{
    internal abstract class FactoryBase
    {
        public abstract DatabaseCommand Create(CommandLineArguments args);
        protected abstract IEnumerable<string> GetVerbs();

        public bool Accepts(string commandName)
        {
            commandName = commandName.ToLower();
            foreach (var verb in GetVerbs())
                if (verb.StartsWith(commandName))
                    return true;
            return false;
        }

        public abstract string Usage { get; }
    }
}