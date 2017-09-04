using System;
using Nin.Command;
using NUnit.Framework;

namespace Test.Integration.Nin.Database
{
    public class UsageTest
    {
        [Test]
        public void UnknownCommandPrintsUsage()
        {
            Assert.Throws<Exception>( () => DatabaseCommand.Parse(new[] { "host schema unknown" }));
        }
    }
}