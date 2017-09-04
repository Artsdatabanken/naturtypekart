using System;
using System.Diagnostics;
using Nin.Common.Diagnostic.Writer;

namespace Nin.Common.Diagnostic
{
    public abstract class BufferedLogWriter : IWriteLogEntries
    {
        readonly LogQueue pendingQueue = new LogQueue();

        int currentFlushIntervalMs = MinimumFlushIntervalMs;
        private const int MinimumFlushIntervalMs = 200;
        private const int MaximumFlushIntervalMs = 60000;
        private DateTime nextFlush;

        protected abstract void Flush(LogEntry logEntry);

        public void Write(LogEntry le)
        {
            if (le.Priority >= MinimumPriority)
                pendingQueue.Enqueue(le);
        }

        public void Flush()
        {
            if (DateTime.UtcNow < nextFlush) return;

            if (SucceededInFlushing())
                currentFlushIntervalMs = MinimumFlushIntervalMs;
            else
                currentFlushIntervalMs = Math.Min(MaximumFlushIntervalMs, currentFlushIntervalMs * 2);
            nextFlush = DateTime.UtcNow.AddMilliseconds(currentFlushIntervalMs);
        }

        bool SucceededInFlushing()
        {
            LogEntry item;
            while (pendingQueue.TryDequeue(out item))
            {
                try
                {
                    Flush(item);
                }
                catch (Exception caught)
                {
                    Debug.WriteLine("Log write delayed, retrying later. " + caught.Message);
                    pendingQueue.Enqueue(item);
                    return false;
                }
            }
            return true;
        }

        public LogPriority MinimumPriority { get; set; }
    }
}