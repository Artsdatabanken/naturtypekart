using Common.Diagnostic;
using Nin.Common;
using Nin.Diagnostic.Writer;

namespace Nin.Command
{
    public class SysLogCommand : DatabaseCommand
    {
        public override void Execute()
        {
            string output = SyslogTableLogWriter.Dump(minimum, new AnsiLogReport());
            System.Console.WriteLine(output);
        }

        public SysLogCommand(LogPriority minimum)
        {
            this.minimum = minimum;
        }

        protected override string GetDescription()
        {
            return "Dumping syslog events having minimum priority '" + minimum + "' (" + (int)minimum + ").";
        }

        private readonly LogPriority minimum;
    }
}