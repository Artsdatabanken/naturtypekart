using System;
using System.Diagnostics;
using Nin.Common;
using Nin.Common.Diagnostic;

namespace Nin.Diagnostic
{
    /// <summary>
    /// Log hack mens vi venter på en løsning som er vakker og som kan brukes globale
    /// </summary>
    public static class Log
    {
        public static void e(string tag, Exception exception)
        {
            Write(tag, LogPriority.Error, exception.ToString());
        }

        public static void v(string tag, string message)
        {
            Write(tag, LogPriority.Verbose, message);
        }

        public static void i(string tag, string message)
        {
            Write(tag, LogPriority.Info, message);
        }

        public static void w(string tag, string message)
        {
            Write(tag, LogPriority.Warn, message);
        }

        public static void d(string tag, string message)
        {
            Write(tag, LogPriority.Debug, message);
        }

        public static void c(string tag, string message)
        {
            Write(tag, LogPriority.Crazy, message);
        }

        public static void Write(string tag, LogPriority logPriority, string msg)
        {
            var le = new LogEntry(tag, logPriority, msg);
            Enqueue(le);
        }

        private static void Enqueue(LogEntry logEntry)
        {
            Debug.WriteLine(logEntry);
            Queue.Enqueue(logEntry);
        }

        static readonly LogQueue Queue = new LogQueue();
        static readonly LogDispatcher Dispatcher = new LogDispatcher(Queue);

        public static void Suspend()
        {
            Dispatcher.Suspend();
        }

        public static void Resume()
        {
            Dispatcher.Resume();
        }

        public static void Flush()
        {
            Dispatcher.FlushAll();
        }
    }
}