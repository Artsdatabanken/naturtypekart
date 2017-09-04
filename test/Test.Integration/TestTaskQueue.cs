using System;
using Nin.Tasks;

namespace Test.Integration
{
    /// <summary>
    /// Wrapper for integration tests in need of TaskQueue 
    /// </summary>
    public static class TestTaskQueue
    {
        static readonly NinServiceContext Context = new NinServiceContext();

        public static bool? IsEmpty => TaskQueue.PeekNext() == Task.Idle;

        public static void Enqueue(Task task)
        {
            task.Id = Queue.Enqueue(task);
            task.Created = DateTime.Now;
        }

        public static Task Enqueue(string taskType, string arguments)
        {
            int taskId = Queue.Enqueue(taskType, arguments);
            return TaskQueue.Read(taskId);
        }

        private static void Process(Task task)
        {
            TaskQueue.Process(Context, task);
        }

        static TestTaskQueue()
        {
            Queue = new TaskQueue();
        }

        private static readonly TaskQueue Queue;

        public static Task PeekNext()
        {
            return TaskQueue.PeekNext();
        }

        public static void Remove(Task task)
        {
            TaskQueue.Remove(task);
        }

        public static void ProcessTask(Task task)
        {
            Enqueue(task);
            Process(task);
        }
    }
}