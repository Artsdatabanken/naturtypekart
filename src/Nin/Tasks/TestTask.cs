using System;

namespace Nin.Tasks
{
    public class TestTask : Task
    {
        public TestTask()
        {
            Created = DateTime.Now;
        }

        public TestTask(int id) : this()
        {
            Id = id;
        }

        public override void Execute(NinServiceContext context)
        {
        }
    }
}