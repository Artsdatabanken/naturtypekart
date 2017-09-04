using System;
using System.Threading;

namespace Nin.Common.Diagnostic
{
    public class LogEntry
    {
        public LogEntry(string tag, LogPriority priority, string message)
        {
            Tag = tag;
            Priority = priority;
            Message = message;
            Created = DateTime.Now;
            ThreadId = Thread.CurrentThread.ManagedThreadId; // TODO: What is different from GetCurrentThreadId
        }

        public DateTime Created;
        public string Tag;
        public LogPriority Priority;
        public string Message;
        public int ThreadId;
        
        public override string ToString()
        {
            return $"{Created} {Tag} {Priority} {Message}";
        }
    }
}