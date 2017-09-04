using Nin.Configuration;
using Nin.IO.SqlServer;

namespace Nin.Tasks
{
    public class SqlCommandTask : Task
    {
        public string Sql;

        public SqlCommandTask()
        {
        }

        public SqlCommandTask(string sql)
        {
            Sql = sql;
        }

        public override void Execute(NinServiceContext context)
        {
            using (var sql = new SqlStatement(Sql, Config.Settings.ConnectionString))
            {
                sql.ExecuteNonQuery();
            }
        }
    }
}