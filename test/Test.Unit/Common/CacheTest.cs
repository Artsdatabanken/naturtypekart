using Nin.Common.Map.Tiles.Stores;
using NUnit.Framework;

namespace Nin.Test.Unit.Common
{
    public class CacheTest
    {
        private FakeBackend backend;
        private Cache<string> cache;

        [Test]
        public void SingleLoad()
        {
            cache.Load("1");
            Assert.AreEqual("1", backend.Loads);
        }

        [Test]
        public void RepeatedLoad_SingleCallToBackend()
        {
            cache.Load("1");
            cache.Load("1");
            Assert.AreEqual("1", backend.Loads);
        }

        [Test]
        public void MultipleKeys()
        {
            cache.Load("1");
            cache.Load("2");
            cache.Load("1");
            Assert.AreEqual("1,2", backend.Loads);
        }

        [Test]
        public void SaveThenLoad()
        {
            cache.Save("1", "S1");
            Assert.AreEqual("S1", cache.Load("1"));
        }

        [Test]
        public void MultipleSaves()
        {
            cache.Save("1", "S1");
            cache.Save("2", "S1");
            cache.Save("1", "S1");
            Assert.AreEqual("1,2,1", backend.Saves);
        }

        [Test]
        public void SaveThenMultipleLoad()
        {
            cache.Save("11", "S1");
            cache.Load("12");
            cache.Load("11");
            Assert.AreEqual("12", backend.Loads);
        }

        [SetUp]
        public void Setup()
        {
            backend = new FakeBackend();
            cache = new Cache<string>(backend);
        }
    }

    public class FakeBackend : IPersistStuff<string>
    {
        public void Save(string key, string tile)
        {
            if (!string.IsNullOrEmpty(Saves))
                Saves += ",";
            Saves += key;
        }

        public string Load(string key)
        {
            if (!string.IsNullOrEmpty(Loads))
                Loads += ",";
            Loads += key;
            return "V"+key;
        }

        public string Loads = "";
        public string Saves = "";
    }
}