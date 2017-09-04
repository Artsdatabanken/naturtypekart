using Nin.Common.Diagnostic.Writer;

namespace Nin.Common.Diagnostic
{
    public abstract class UnbufferedLogWriter : IWriteLogEntries
    {
        public abstract void Write(LogEntry entry);

        public void Flush()
        {
        }

        public LogPriority MinimumPriority { get; set; }
    }
}