using System.Net.Mail;
using Nin.Configuration;

namespace Nin.Tasks
{
    public class SendEmailTask : Task
    {
        public string Body;
        public string Subject;
        public string To;

        public override void Execute(NinServiceContext context)
        {
            var settings = Config.Settings.ExternalDependency.Email;
            var smtp = new SmtpClient(settings.Server, 25)
            {
                UseDefaultCredentials = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = false
            };

            var mail = new MailMessage {From = new MailAddress(settings.SenderEmail, settings.SenderName)};
            mail.To.Add(new MailAddress(To));
            mail.Subject = Subject;
            mail.Body = Body;
            smtp.Send(mail);
        }
    }
}