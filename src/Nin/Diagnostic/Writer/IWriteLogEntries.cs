namespace Nin.Common.Diagnostic.Writer
{
    public interface IWriteLogEntries
    {
        void Write(LogEntry entry);
        void Flush();
        LogPriority MinimumPriority { get; set; }
    }
}