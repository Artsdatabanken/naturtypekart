using System;
using Common.Diagnostic;
using Nin.Common;
using Nin.Common.Diagnostic;
using Nin.Configuration;
using Nin.IO.SqlServer;

namespace Nin.Diagnostic.Writer
{
    public class SyslogTableLogWriter : BufferedLogWriter
    {
        protected override void Flush(LogEntry logEntry)
        {
            using (var sql = new SqlStatement(
                    "INSERT INTO SysLog (Created, Priority, Tag, Msg) VALUES (@Created, @Priority, @tag, @msg)",
                    Config.Settings.ConnectionString))
            {
                sql.AddParameter("@Created", logEntry.Created);
                sql.AddParameter("@Priority", logEntry.Priority);
                sql.AddParameter("@Tag", logEntry.Tag);
                sql.AddParameter("@Msg", logEntry.Message);
                sql.ExecuteNonQuery();
            }
        }

        public static string Dump(LogPriority logPriority, LogReport report, int count = 1000)
        {
            string sql =
                $"SELECT TOP {count} created, priority, tag, msg FROM SysLog WHERE Priority >= @priority ORDER BY created DESC";
            using (var q = new SqlStatement(sql, Config.Settings.ConnectionString))
            {
                q.AddParameter("@Priority", logPriority);
                using (var reader = q.ExecuteReader())
                {
                    if (reader.HasRows) Environment.ExitCode = 1;
                    var r = LogReporter.Write(reader, report);
                    return r;
                }
            }
        }
    }
}