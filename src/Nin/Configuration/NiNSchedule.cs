using System;
using System.Collections.Generic;

namespace Nin.Configuration
{
    public interface IEnqueueTasks
    {
        int Enqueue(string taskType, string payload);
    }

    public class Schedule : List<ScheduledTask>
    {
        public void QueueDueTasks(IEnqueueTasks queue)
        {
            foreach (var task in this)
                if (task.NextRun < DateTime.Now)
                    QueueScheduledTask(queue, task);
        }

        private static void QueueScheduledTask(IEnqueueTasks queue, ScheduledTask task)
        {
            queue.Enqueue(task.TaskName, task.Payload);
            task.LastRun = DateTime.Now;
        }

        private void AddDefault()
        {
            Add(new ScheduledTask("RefreshCodeTree", TimeSpan.FromHours(6), "{ KodetreType: \"Naturtyper\"}"));
            Add(new ScheduledTask("RefreshCodeTree", TimeSpan.FromHours(6), "{ KodetreType: \"Naturvariasjon\"}"));
            Add(new ScheduledTask("MonitorService", TimeSpan.FromMinutes(15), "{ Url:\"http://it-webadbtest01.it.ntnu.no/NinDocument/DataDelivery/GetListOfImportedDataDeliveries\" }"));
        }

        public void AddDefaultsIfEmpty()
        {
            if (Count <= 0)
                AddDefault();
        }
    }
}