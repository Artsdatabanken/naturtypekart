using System.Threading;

namespace Nin.Tasks
{
    public class IdleTask : Task
    {
        public override void Execute(NinServiceContext context)
        {
            Thread.Sleep(4000);
        }
    }
}