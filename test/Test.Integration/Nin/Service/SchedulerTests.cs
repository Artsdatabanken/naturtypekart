using System;
using System.Collections.Generic;
using Nin.Configuration;
using Nin.Engine;
using NUnit.Framework;

namespace Nin.Test.Integration.NiN.Service
{
    public class SchedulerTests
    {
        [Test]
        public void DueTaskGetScheduled()
        {
            TestQueue queue = new TestQueue();
            var scheduler = new TestScheduler("SavePrincessLeia", TimeSpan.FromDays(999));
            scheduler.ScheduleDueJobsOn(queue);
            Assert.True(queue.ToString()=="SavePrincessLeia");
        }
    }

    public class TestScheduler : Scheduler
    {
        public TestScheduler(string taskName, TimeSpan interval) : 
            base(new TestSchedule(taskName, interval))
        {
        }
    }

    public class TestQueue : IEnqueueTasks
    {
        readonly List<string> tasks = new List<string>();
        public int Enqueue(string taskType, string arguments)
        {
            tasks.Add(taskType);
            return 0;
        }

        public override string ToString()
        {
            return string.Join(",", tasks.ToArray());
        }
    }

    public class TestSchedule : Schedule
    {
        public TestSchedule(string taskName, TimeSpan interval)
        {
            Add(new ScheduledTask(taskName, interval, ""));
        }
    }
}