using System;
using Nin.Common;
using NUnit.Framework;

namespace Nin.Test.Unit.Common
{
    public class TimeSpanStringTest
    {
        [Test]
        public void OneSecond()
        {
            var span = TimeSpanString.ToString(TimeSpan.FromSeconds(1));
            Assert.AreEqual("1 second", span);
        }

        [Test]
        public void FourHours()
        {
            var span = TimeSpanString.ToString(TimeSpan.FromHours(4));
            Assert.AreEqual("4 hours", span);
        }

        [Test]
        public void OneDay2Seconds()
        {
            var span = TimeSpanString.ToString(new TimeSpan(1, 0, 0, 2));
            Assert.AreEqual("1 day, 2 seconds", span);
        }

        [Test]
        public void AllParts()
        {
            var span = TimeSpanString.ToString(new TimeSpan(1, 2, 3, 4, 5));
            Assert.AreEqual("1 day, 2 hours, 3 minutes, 4 seconds", span);
        }
    }
}