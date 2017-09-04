using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Microsoft.SqlServer.Types;
using Nin.Configuration;
using Nin.Diagnostic;

namespace Nin.IO.SqlServer
{
    public class SqlStatement : IDisposable
    {
        public void Dispose()
        {
            Cmd.Connection.Dispose();
            Cmd.Dispose();
        }

        internal void AddParameter(SqlParameter sqlParameter)
        {
            Cmd.Parameters.Add(sqlParameter);
        }

        public SqlDataReader ExecuteReader()
        {
            return Cmd.ExecuteReader();
        }

        public int ExecuteNonQuery()
        {
            return Cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar()
        {
            return Cmd.ExecuteScalar();
        }

        public void AddParameter(string fieldName, Guid value)
        {
            Cmd.Parameters.Add(fieldName, SqlDbType.UniqueIdentifier).Value = value;
        }

        public void AddParameter(string fieldName, string value)
        {
            var v = value ?? (object)DBNull.Value;
            Cmd.Parameters.Add(fieldName, SqlDbType.VarChar).Value = v;
        }

        public void AddParameter(string fieldName, int value)
        {
            Cmd.Parameters.Add(fieldName, SqlDbType.Int).Value = value;
        }

        public void AddParameterNull(string fieldName, int value)
        {
            object o = DBNull.Value;
            if (value > 0) o = value;
            Cmd.Parameters.Add(fieldName, SqlDbType.Int).Value = o;
        }

        public void AddParameter(string fieldName, DateTime value)
        {
            Cmd.Parameters.Add(fieldName, SqlDbType.DateTime).Value = value;
        }

        public void AddParameter(string fieldName, double value)
        {
            Cmd.Parameters.Add(fieldName, SqlDbType.Real).Value = value;
        }

        public void AddParameter(string fieldName, int? value)
        {
            var v = value ?? (object)DBNull.Value;
            Cmd.Parameters.Add(fieldName, SqlDbType.Int).Value = v;
        }

        public void AddParameter(string fieldName, DateTime? value)
        {
            object o = DBNull.Value;
            if (value != null)
                o = value;
            Cmd.Parameters.Add(fieldName, SqlDbType.DateTime).Value = o;
        }

        public void AddParameter(string fieldName, SqlGeometry value)
        {
            object o = DBNull.Value;
            if (value != null)
                o = value.Serialize();
            Cmd.Parameters.Add(fieldName, SqlDbType.VarBinary).Value = o;
        }

        public void AddParameter(string fieldName, SqlDbType sqlDbType, object value)
        {
            Cmd.Parameters.Add(fieldName, sqlDbType).Value = value;
        }

        public void AddParameter(string fieldName, Enum value)
        {
            Cmd.Parameters.Add(fieldName, SqlDbType.Int).Value = value;
        }

        public static void Execute(string sql)
        {
            using (var q = new SqlStatement(sql, Config.Settings.ConnectionString))
                q.ExecuteNonQuery();
        }

        private void OpenConnection()
        {
            int delay = 200;
            const int MaxRetryCount = 10;
            int retries = MaxRetryCount;
            while (true)
            {
                try
                {
                    Cmd.Connection.Open();
                    if(retries< MaxRetryCount)
                        Log.w("DB", $"Connection success after {MaxRetryCount - retries} reattempts.");
                    return;
                }
                catch (Exception caught)
                {
                    if (retries == 0 || !caught.Message.Contains("timeout"))
                    {
                        var ds = new SqlConnectionStringBuilder(Cmd.Connection.ConnectionString).DataSource;
                        throw new Exception($"Kan ikke koble til database '{ds}': " + caught.Message);
                    }

                    Log.e("DB", caught);
                    retries--;
                    Thread.Sleep(delay);
                    delay *= 2;
                }
            }
        }

        public SqlStatement(string commandText) : this(commandText, Config.Settings.ConnectionString)
        {
        }

        public SqlStatement(string commandText, string connectionString)
        {
            Cmd = new SqlCommand(commandText, new SqlConnection(connectionString));
            OpenConnection();
        }

        public int CommandTimeout
        {
            set => Cmd.CommandTimeout = value;
        }

        protected readonly SqlCommand Cmd;
    }
}