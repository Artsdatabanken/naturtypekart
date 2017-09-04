using System;
using Nin.Configuration;

namespace Nin.Engine
{
    public class Scheduler
    {

        private DateTime lastScheduleRun;
        private readonly TimeSpan scheduleInterval = new TimeSpan(0, 1, 0);
        readonly Schedule schedule;

        public Scheduler(Schedule schedule)
        {
            this.schedule = schedule;
        }

        public void ScheduleDueJobsOn(IEnqueueTasks queue)
        {
            if (DateTime.Now.Subtract(lastScheduleRun) < scheduleInterval) return;

            schedule.QueueDueTasks(queue);
            lastScheduleRun = DateTime.Now;
        }
    }
}