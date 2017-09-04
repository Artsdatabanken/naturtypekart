using System;

namespace Nin.Common.Diagnostic.Writer
{
    public class ConsoleLogWriter : UnbufferedLogWriter
    {
        public override void Write(LogEntry entry)
        {
            Console.WriteLine(entry.ToString());
        }
    }
}