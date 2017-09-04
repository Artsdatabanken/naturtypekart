using System;
using System.IO;
using System.Threading;
using Nin.Common;
using Nin.Configuration;
using Nin.Engine;
using Nin.Tasks;

namespace Engine
{
    public class EngineProcess
    {
        public void Start()
        {
            abort = false;
            var thread = new Thread(Run) {IsBackground = true};
            thread.Start();
        }

        public void Run()
        {
            Log(LogPriority.Info, "Nin Service starting...");


            foreach (var layer in Config.Settings.Map.Layers)
                Log(LogPriority.Info, $"Map layer: {layer}");
            var schedule = Config.Settings.Schedule;
            schedule.AddDefaultsIfEmpty();
            scheduler = new Scheduler(schedule);
            queue = new TaskQueue();
            context = new NinServiceContext();
            Log(LogPriority.Verbose, "Hello, my name is " + Environment.UserName + ".");

            RunLoop();
        }

        private void RunLoop()
        {
            while (!abort)
            {
                ProcessQueue();
                Thread.Sleep(retryDelay);
            }
            Log(LogPriority.Info, "Stopping ");
        }

        private void ProcessQueue()
        {
            try
            {
                ProcessQueuedJobs();
                scheduler.ScheduleDueJobsOn(queue);
            }
            catch (Exception caught)
            {
                Log(LogPriority.Error, caught.ToString());
                Thread.Sleep(retryDelay);
                retryDelay = new TimeSpan((long) (retryDelay.Ticks*1.5));
                if (retryDelay > maximumRetryDelay)
                    retryDelay = maximumRetryDelay;
            }
        }

        private void ProcessQueuedJobs()
        {
            var hasMoreWork = true;
            while (hasMoreWork)
            {
                if (abort) break;
                hasMoreWork = TaskQueue.ProcessNext(context);
                retryDelay = minimumDelay;
            }
        }

        private static void Log(LogPriority logPriority, string msg)
        {
            try
            {
                Nin.Diagnostic.Log.Write("SRV", logPriority, msg);
            }
            catch (Exception caught)
            {
                var logPath = Path.GetTempFileName() + ".xxx";
                File.WriteAllText(logPath, caught.ToString());
                throw;
            }
        }

        public void Stop()
        {
            abort = true;
        }

        private static bool abort;
        private readonly TimeSpan maximumRetryDelay = new TimeSpan(0, 30, 0);
        private readonly TimeSpan minimumDelay = new TimeSpan(0, 0, 10);
        private NinServiceContext context;
        private TaskQueue queue;
        private TimeSpan retryDelay = new TimeSpan(0, 0, 10);
        private Scheduler scheduler;
    }
}