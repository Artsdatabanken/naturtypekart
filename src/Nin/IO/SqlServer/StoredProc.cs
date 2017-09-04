using System.Data;
using System.Data.SqlClient;

namespace Nin.IO.SqlServer
{
    public class StoredProc : SqlStatement
    {
        public StoredProc(string commandText, string connectionString) : base(commandText, connectionString)
        {
            Cmd.CommandType = CommandType.StoredProcedure;
        }

        public SqlParameter AddReturnParameter(string field, SqlDbType sqlDbType)
        {
            var param = Cmd.Parameters.Add(field, sqlDbType);
            param.Direction = ParameterDirection.ReturnValue;
            return param;
        }
    }
}