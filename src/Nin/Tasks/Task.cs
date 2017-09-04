using System;
using System.Collections.Generic;
using Nin.Diagnostic;
using Raven.Imports.Newtonsoft.Json;

namespace Nin.Tasks
{
    public abstract class Task
    {
        [JsonIgnore]
        public int Id;

        [JsonIgnore]
        public DateTime Created;

        public static readonly Task Idle = new IdleTask();
        public static readonly Task Failed = new IdleTask();

        public static Task Create(string taskType, int id, string json)
        {
            try
            {
                var task = Create(taskType, json);
                task.Id = id;
                return task;
            }
            catch (Exception caught)
            {
                throw new Exception($"Oh man, I'm really struggling to read task '{taskType}' id #{id}: {json}", caught);
            }
        }

        private static Task Create(string taskTypeName, string json)
        {
            Type taskType;
            if (!TaskTypes.TryGetValue(taskTypeName, out taskType))
                throw new Exception("Unknown task type '" + taskTypeName + "'.");
            return Create(taskType, json);
        }

        static Task Create(Type type, string json)
        {
            return (Task)JsonConvert.DeserializeObject(json, type);
        }


        public string Type() => GetType().Name.Replace("Task", "");

        static readonly Dictionary<string, Type> TaskTypes = new Dictionary<string, Type>();

        static Task()
        {
            AddTaskType(new TileAreaTask());
            AddTaskType(new SendEmailTask());
            AddTaskType(new RefreshCodeTreeTask());
            AddTaskType(new SqlCommandTask());
            AddTaskType(new MonitorServiceTask());
            AddTaskType(new TestTask());
            AddTaskType(new IdleTask());
        }

        private static void AddTaskType(Task task)
        {
            TaskTypes.Add(task.Type(), task.GetType());
        }

        public string Serialize()
        {
            try
            {
                var settings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
                return JsonConvert.SerializeObject(this,settings);
            }
            catch (Exception)
            {
                Log.w("TASK", $"Serialize task {Type()} #{Id} failed.");
                throw;
            }
        }

        public abstract void Execute(NinServiceContext context);

        public override string ToString()
        {
            return Type() + ": " + Serialize();
        }
    }
}