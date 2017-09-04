using System;
using System.Collections.Generic;
using Nin.Common.Diagnostic.Writer;
using Nin.Configuration;
using Nin.Diagnostic.Writer;

namespace Nin.Diagnostic
{
    class LogWriterList : List<IWriteLogEntries>
    {
        public static LogWriterList Create(IEnumerable<Logger> diagnosticLogging)
        {
            LogWriterList r = new LogWriterList();
            foreach (var logger in diagnosticLogging)
            {
                var writer = Create(logger);
                writer.MinimumPriority = logger.MinimumPriority;
                r.Add(writer);
            }
            return r;
        }

        private static IWriteLogEntries Create(Logger logger)
        {
            switch (logger.Name.ToLower())
            {
                case "console": return new ConsoleLogWriter();
                case "debug": return new DebugLogWriter();
                case "pushover": return new PushoverLogWriter();
                case "syslog": return new SyslogTableLogWriter();
                case "test": return new TestLogWriter();
                default: throw new Exception("Unknown logger name '" + logger.Name + "'.");
            }
        }
    }
}