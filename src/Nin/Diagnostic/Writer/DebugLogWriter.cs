using System.Diagnostics;

namespace Nin.Common.Diagnostic.Writer
{
    public class DebugLogWriter : UnbufferedLogWriter
    {
        public override void Write(LogEntry entry)
        {
            Debug.WriteLine(entry.ToString());
        }
    }
}