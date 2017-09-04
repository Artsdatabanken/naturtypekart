using System.Collections.Concurrent;

namespace Nin.Common.Diagnostic
{
    public class LogQueue
    {
        readonly ConcurrentQueue<LogEntry> queue = new ConcurrentQueue<LogEntry>();
        const int Size = 200;

        public int Count => queue.Count;

        public void Enqueue(LogEntry logEntry)
        {
            if (queue.Count >= Size)
            {
                LogEntry r;
                queue.TryDequeue(out r);
            }
            queue.Enqueue(logEntry);
        }

        public bool TryDequeue(out LogEntry r)
        {
            return queue.TryDequeue(out r);
        }
    }
}