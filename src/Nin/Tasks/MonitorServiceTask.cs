using System;
using Common.Diagnostic.Network.Web;

namespace Nin.Tasks
{
    public class MonitorServiceTask : Task
    {
        public string Url;

        public override void Execute(NinServiceContext context)
        {
            try
            {
                Http.Get(Url);
            }
            catch (Exception e)
            {
                Notify(context, e);
            }
        }

        private void Notify(NinServiceContext context, Exception exception)
        {
            var email = new SendEmailTask
            {
                To = "bjorn.reppen@artsdatabanken.no",
                Subject = "Service down: " + exception.Message,
                Body = Url + Environment.NewLine + Environment.NewLine + exception
            };
            context.TaskQueue.Enqueue(email);
        }
    }
}