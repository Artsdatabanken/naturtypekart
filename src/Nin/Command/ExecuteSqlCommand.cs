using Nin.Configuration;
using Nin.Diagnostic;
using Nin.IO.SqlServer;

namespace Nin.Command
{
    public class ExecuteSqlCommand : DatabaseCommand
    {
        public override void Execute()
        {
            using (var q = new SqlStatement(sql, Config.Settings.ConnectionString))
                q.ExecuteNonQuery();
            Log.i("DB", "Executed SQL successfully.");
        }

        public ExecuteSqlCommand(string sql)
        {
            this.sql = sql;
        }

        protected override string GetDescription()
        {
            return "Executing SQL: " + sql;
        }
        readonly string sql;
    }
}