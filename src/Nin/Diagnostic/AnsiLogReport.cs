using Nin.Common;
using Nin.Common.Diagnostic.TextMode;

namespace Common.Diagnostic
{
    public class AnsiLogReport : LogReport
    {
        public override void WriteLine(LogPriority logPriority, string[] msg)
        {
            string line = string.Join(" ", msg);
            switch (logPriority)
            {
                case LogPriority.Error:
                    sb.AppendLine(AnsiConsole.WriteLine2(AnsiColor.Red, line));
                    break;
                case LogPriority.Warn:
                    sb.AppendLine(AnsiConsole.WriteLine2(AnsiColor.Yellow, line));
                    break;
                default:
                    System.Console.WriteLine(line);
                    break;
            }
        }

        public override void Start()
        {
        }

        public override void End()
        {
        }
    }
}