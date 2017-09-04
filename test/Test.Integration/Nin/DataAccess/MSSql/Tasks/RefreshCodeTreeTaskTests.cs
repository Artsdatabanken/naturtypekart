using Nin.Common;
using Nin.Tasks;
using NUnit.Framework;

namespace Test.Integration.Nin.DataAccess.MSSql.Tasks
{
    public class RefreshCodeTreeTaskTests
    {
        [Test]
        public void RefreshCodeTree_AlleKoder()
        {
            RefreshCodeTree(KodetreType.AlleKoder);
        }

        [Test]
        public void RefreshCodeTree_Variasjon()
        {
            RefreshCodeTree(KodetreType.Variasjon);
        }

        private static void RefreshCodeTree(KodetreType kodetreType)
        {
            var t = new RefreshCodeTreeTask();
            t.KodetreType = kodetreType;

            TestTaskQueue.ProcessTask(t);
        }
    }
}