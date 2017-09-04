using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nin.Configuration;
using NUnit.Framework;

namespace Test.Integration.Nin.Common.Configuration
{
    public class EnvironmentVariablesTest 
    {
        [Test]
        public void OverrideFromEnvironmentVariables()
        {
            JObject cfg = JObject.Parse(@"{'A': { 'B': 'default' } }");
            Environment.SetEnvironmentVariable("A.B", "override");
            EnvironmentVariables.AddOverridesTo(cfg);
            var actual = JsonConvert.SerializeObject(cfg, Formatting.None);
            Assert.AreEqual("{'A':{'B':'override'}}".Replace("'", "\""), actual);
        }

        [Test]
        public void OverrideFromConnectionStringEnvironmentVariable()
        {
            Environment.SetEnvironmentVariable("ConnectionString", "override");
            var settings = Config.LoadFrom(".");
            Assert.AreEqual("override", settings.ConnectionString);
        }
    }
}