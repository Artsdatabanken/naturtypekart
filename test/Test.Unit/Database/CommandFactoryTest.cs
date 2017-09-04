using Nin.Command;
using Nin.Command.Factory;
using NUnit.Framework;

namespace Test.Unit.Database
{
    public class CommandFactoryTest
    {
        [Test]
        public void Create()
        {
            var cmd = Parse("create nin");
            Assert.True(cmd is CreateDatabaseCommand);
        }

        [Test]
        public void Upgrade()
        {
            var cmd = Parse("upgrade .");
            Assert.True(cmd is UpgradeCommand);
        }

        [Test]
        public void ExecuteSql()
        {
            var cmd = Parse("exec DROP TABLE BOBBY");
            Assert.True(cmd is ExecuteSqlCommand);
        }

        [Test]
        public void Syslog()
        {
            var cmd = Parse("syslog");
            Assert.True(cmd is SysLogCommand);
        }

        [Test]
        public void ImportArea()
        {
            var cmd = Parse("importarea file 4236 kommune");
            Assert.True(cmd is ImportAreaCommand);
        }

        [Test]
        public void ImportGrid()
        {
            var cmd = Parse("importgrid file 32633 SSB001KM");
            Assert.True(cmd is ImportGridCommand);
        }

        [Test]
        public void ImportAreaLayer()
        {
            var cmd = Parse("importarealayer file");
            Assert.True(cmd is ImportAreaLayerCommand);
        }

        private static DatabaseCommand Parse(string args)
        {
            return new CommandFactory().Parse(args.Split(' '));
        }
    }
}