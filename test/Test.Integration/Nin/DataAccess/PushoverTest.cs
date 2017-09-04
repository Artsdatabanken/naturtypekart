using Nin.Diagnostic;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess
{
    public class PushoverTest
    {
        [Test][Ignore("Bråker litt mye denne.")]
        public void Push()
        {
            Pushover.SendNotification("Pushovertest OK");
        }
    }
}