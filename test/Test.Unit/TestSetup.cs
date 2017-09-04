using Nin.Common;
using Nin.Configuration;
using NUnit.Framework;

namespace Test.Unit
{
    [SetUpFixture]
    public class TestSetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            Config.Settings = new UnitTestConfig();
        }

        public class UnitTestConfig : Config
        {
            public UnitTestConfig()
            {
                Diagnostic.Logging = new[] {new Logger("Test", LogPriority.Crazy), };
            }
        }
    }
}