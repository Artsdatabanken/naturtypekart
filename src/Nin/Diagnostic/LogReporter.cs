using System.Data.SqlClient;
using Nin.Common;

namespace Common.Diagnostic
{
    internal static class LogReporter
    {
        public static string Write(SqlDataReader reader, LogReport writer)
        {
            writer.Start();
            while (reader.Read())
            {
                LogPriority logPriority = (LogPriority)reader["Priority"];
                var line = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                    line[i] += reader[i];
                writer.WriteLine(logPriority, line);
            }
            writer.End();
            return writer.GetOutput();
        }
    }
}