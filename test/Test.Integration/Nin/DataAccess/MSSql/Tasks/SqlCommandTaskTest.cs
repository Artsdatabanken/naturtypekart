using Nin.Tasks;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess.MSSql.Tasks
{
    public class SqlCommandTaskTest
    {
        [Test]
        public void SqlCommandTask()
        {
            TestTaskQueue.ProcessTask(new SqlCommandTask("SELECT COUNT(*) FROM SysLog"));
        }
    }
}