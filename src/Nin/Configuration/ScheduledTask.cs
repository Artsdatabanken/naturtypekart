using System;

namespace Nin.Configuration
{
    public class ScheduledTask
    {
        public ScheduledTask(string taskName, TimeSpan interval, string payload)
        {
            TaskName = taskName;
            Interval = interval;
            Payload = payload;
        }

        public TimeSpan Interval;
        public string TaskName;
        public string Payload;
        public DateTime LastRun;
        public DateTime NextRun => LastRun.Add(Interval);
    }
}