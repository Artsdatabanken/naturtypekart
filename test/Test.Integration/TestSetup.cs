using System;
using System.IO;
using Nin.Configuration;
using Nin.Diagnostic;
using Nin.IO.SqlServer;
using NUnit.Framework;

namespace Test.Integration
{
    [SetUpFixture]
    public class TestSetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            Config.Settings = Config.Load();
            DeployEmptyDatabase();
        }

        private static void DeployEmptyDatabase()
        {
            Console.WriteLine("Running tests on: " + Config.Settings.ConnectionString);
            try
            {
                var schemaScriptDirectory = GetDatabasePath();
                SqlServerDatabase.WipeAndCreate(schemaScriptDirectory);
                //SqlServerDatabase.UpgradeExisting(schemaScriptDirectory);
            }
            catch (Exception caught)
            {
                Log.e("DB", caught);
                throw;
            }
        }

        private static string GetDatabasePath()
        {
            var findDirectoryInTree = FileLocator.FindFileInTree(@"Database\001_Schema.sql");
            return Path.GetDirectoryName(findDirectoryInTree);
        }
    }
}