using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Nin.Common.Diagnostic;
using Nin.Common.Diagnostic.Writer;
using Nin.Configuration;

namespace Nin.Diagnostic
{
    class LogDispatcher
    {
        private readonly LogQueue incoming;
        private bool isEnabled;

        public LogDispatcher(LogQueue incoming)
        {
            this.incoming = incoming;
            Config.Initialized += Initialized;
            if (Config.IsInitialized)
                Initialize(Config.Settings.Diagnostic.Logging);
        }

        private void Initialized(object sender, EventArgs eventArgs)
        {
            string baseJson = JsonConvert.SerializeObject(this);
            Console.WriteLine(baseJson);
            Initialize(Config.Settings.Diagnostic.Logging);
        }

        private void Initialize(IEnumerable<Logger> diagnosticLogging)
        {
            writers = LogWriterList.Create(diagnosticLogging);
            if (writers.Count > 0)
                Start();
        }

        private void Flush()
        {
            foreach (IWriteLogEntries writer in Writers)
                writer.Flush();
        }

        private void Start()
        {
            isEnabled = true;
            if (thread != null && thread.IsAlive)
                return;
            lock (typeof(LogDispatcher))
            {
                if (thread != null && thread.IsAlive)
                    return;
                thread = new Thread(ProcessLogQueue)
                {
                    Name = "Log writer",
                    IsBackground = true
                };
                thread.Start();
            }
        }

        private void ProcessLogQueue()
        {
            while (true)
            {
                FlushAll();
                Thread.Sleep(100);
            }
        }

        public void FlushAll()
        {
            if (!isEnabled) return;
            ProcessIncoming();
            Flush();
        }

        private void ProcessIncoming()
        {
            while (true)
            {
                LogEntry entry;
                if (!incoming.TryDequeue(out entry))
                    break;
                foreach (var writer in Writers)
                    writer.Write(entry);
            }
        }

        private LogWriterList writers;
        private Thread thread;

        private LogWriterList Writers
        {
            get
            {
                if (writers != null) return writers;

                if (!Config.IsInitialized) return new LogWriterList();
                return writers;
            }
        }

        /// <summary>
        /// Temporarily stop log queue processing, for example while log destination is being initialized.
        /// </summary>
        public void Suspend()
        {
            isEnabled = false;
        }

        /// <summary>
        /// Resume normal operations
        /// </summary>
        public void Resume()
        {
            isEnabled = true;
        }
    }
}