using System;
using Nin.Tasks;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess.MSSql.Tasks
{
    public class SendEmailTaskTests
    {
        [Test]
        public void SendEmail_ToInvalidRecipient_Fails()
        {
            Assert.Throws<Exception>(() =>
                    SendEmail("invalid@localhost"));
        }

        [Test]
        public void SendEmail()
        {
            SendEmail("bjorn.reppen@artsdatabanken.no");
            //SendEmail("junk@mailinator.com");
        }

        private static void SendEmail(string recipient)
        {
            SendEmailTask t = new SendEmailTask
            {
                To = recipient,
                Subject = "NiN SendEmailTaskTests",
                Body = "Test PASS"
            };

            TestTaskQueue.ProcessTask(t);
        }
    }
}