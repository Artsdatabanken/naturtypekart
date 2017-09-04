using System;
using Nin.Tasks;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess.MSSql.Tasks
{
    public class TaskQueueTests
    {
        [Test]
        public void Task_Process_Invalid_Task()
        {
            var task = TestTaskQueue.Enqueue("invalid", "invalid");
            TestTaskQueue.ProcessTask(task);
        }

        [Test]
        public void Task_Enqueue()
        {
            TestTaskQueue.Enqueue(new TestTask());
        }

        [Test]
        public void Task_Peek()
        {
            var actual = TestTaskQueue.Enqueue("Test", "{}");
            Assert.NotNull(actual);
            Assert.True(actual.GetType() == typeof(TestTask));
        }

        [Test]
        public void Task_Remove()
        {
            var task = new TestTask();
            TestTaskQueue.Enqueue(task);
            TestTaskQueue.Remove(task);
        }

        [Test]
        public void Task_Remove_When_Empty()
        {
            Assert.Throws<Exception>(() => TestTaskQueue.Remove(new TestTask(-1)));
        }

        [Test]
        public void Task_Process()
        {
            TestTaskQueue.ProcessTask(new TestTask());
        }
    }
}