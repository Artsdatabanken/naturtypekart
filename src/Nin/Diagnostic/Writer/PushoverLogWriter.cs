using Nin.Common.Diagnostic;

namespace Nin.Diagnostic.Writer
{
    /// <summary>
    /// Send log messages over Pushover protocol
    /// https://pushover.net/
    /// </summary>
    public class PushoverLogWriter : BufferedLogWriter
    {
        protected override void Flush(LogEntry logEntry)
        {
            Pushover.SendNotification(logEntry.Tag + " " + logEntry.Message);
        }
    }
}