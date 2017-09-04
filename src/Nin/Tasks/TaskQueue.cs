using System;
using System.Diagnostics;
using Nin.Configuration;
using Nin.Diagnostic;
using Nin.IO.SqlServer;

namespace Nin.Tasks
{
    public class TaskQueue : IEnqueueTasks
    {
        public int Enqueue(string taskType, string payload)
        {
            const string sql =
                "INSERT INTO TaskQueue (Action, Payload) OUTPUT INSERTED.TaskQueueId VALUES (@task, @payload)";
            int taskId;
            using (var cmd = new SqlStatement(sql, Config.Settings.ConnectionString))
            {
                cmd.AddParameter("@task", taskType);
                cmd.AddParameter("@payload", payload);
                taskId = (int) cmd.ExecuteScalar();
            }
            return taskId;
        }

        public static Task PeekNext()
        {
            const string sql = "SELECT TOP 1 * FROM TaskQueue"; // WITH (UPDLOCK, READPAST)";
            return ReadTask(sql);
        }

        private static Task ReadTask(string sql)
        {
            using (var sqlCommand = new SqlStatement(sql, Config.Settings.ConnectionString))
            using (var reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = (int) reader[0];
                    var action = (string) reader[1];
                    var payload = (string) reader[2];
                    var task = CreateTask(action, id, payload);
                    task.Created = (DateTime) reader[3];
                    if (task != Task.Failed)
                        return task;
                }
                return Task.Idle;
            }
        }

        public static Task Read(int taskId)
        {
            var sql = "SELECT * FROM TaskQueue WHERE TaskQueueId=" + taskId;
            return ReadTask(sql);
        }

        private static Task CreateTask(string taskType, int id, string json)
        {
            try
            {
                return Task.Create(taskType, id, json);
            }
            catch (Exception caught)
            {
                Log.e("TASK", caught);
                Log.w("TASK", $"Moving failed task {taskType} #{id} to error queue.");
                MoveToErrorQueue(id, caught);
                return Task.Failed;
            }
        }

        private static void MoveToErrorQueue(int taskId, Exception caught)
        {
            SaveToErrorQueue(taskId, caught);
            DeleteTask(taskId);
        }

        private static void SaveToErrorQueue(int taskId, Exception caught)
        {
            const string sql =
                "INSERT INTO dbo.TaskQueueError SELECT TaskQueueId, Action, Payload, @exception, Created, GETDATE() FROM dbo.TaskQueue WHERE TaskQueueId = @taskId";
            using (var cmd = new SqlStatement(sql, Config.Settings.ConnectionString))
            {
                cmd.AddParameter("@taskId", taskId);
                cmd.AddParameter("@exception", caught.ToString());
                cmd.ExecuteNonQuery();
            }
        }

        public int Enqueue(Task task)
        {
            var taskType = task.Type();
            var payload = task.Serialize();

            task.Id = Enqueue(taskType, payload);
            return task.Id;
        }

        public static void Remove(Task task, long elapsedMilliseconds = 0)
        {
            const string sql =
                "INSERT INTO TaskLog (Action, Payload, Created, ElapsedMilliSeconds) VALUES (@action, @payload, @created, @elapsedMilliseconds)";
            using (var cmd = new SqlStatement(sql, Config.Settings.ConnectionString))
            {
                cmd.AddParameter("@action", task.Type());
                cmd.AddParameter("@payload", task.Serialize());
                cmd.AddParameter("@created", task.Created);
                cmd.AddParameter("@elapsedMilliseconds", elapsedMilliseconds);
                cmd.ExecuteNonQuery();
            }

            var taskId = task.Id;
            DeleteTask(taskId);
        }

        private static void DeleteTask(int taskId)
        {
            const string sql = "DELETE FROM TaskQueue WHERE TaskQueueId=@Id";
            using (var cmd = new SqlStatement(sql, Config.Settings.ConnectionString))
            {
                cmd.AddParameter("@id", taskId);
                var r = cmd.ExecuteNonQuery();
                if (r <= 0)
                    throw new Exception("Failed to remove task with id '" + taskId + "'.");
            }
        }

        public static void Wipe()
        {
            const string sql = "DELETE FROM TaskQueue";
            using (var cmd = new SqlStatement(sql, Config.Settings.ConnectionString))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static bool ProcessNext(NinServiceContext context)
        {
            var task = PeekNext();
            return Process(context, task);
        }

        public static bool Process(NinServiceContext context, Task task)
        {
            if (task == Task.Idle) return false;
            var watch = Stopwatch.StartNew();
            try
            {
                task.Execute(context);
            }
            catch (Exception caught)
            {
                MoveToErrorQueue(task.Id, caught);
                throw new Exception(task + " suspended.", caught);
            }
            watch.Stop();
            Remove(task, watch.ElapsedMilliseconds);
            return true;
        }
    }
}