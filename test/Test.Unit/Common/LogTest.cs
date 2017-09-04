using System;
using Nin.Common;
using Nin.Common.Diagnostic.Writer;
using Nin.Diagnostic;
using NUnit.Framework;

namespace Test.Unit.Common
{
    public class LogTest
    {
        [Test]
        public void Log_Info()
        {
            const string message = "Testing logging info message";
            Log.i(Tag, message);

            AssertThatLogWasSaved(message);
        }

        [Test]
        public void Log_Error()
        {
            const string message = "Test logging Exception";
            Log.e(Tag, new Exception(message));

            AssertThatLogWasSaved(message);
        }

        [Test]
        public void Log_Write()
        {
            const string message = "Testing informational message";
            const LogPriority logPriority = LogPriority.Warn;

            Log.Write(Tag, logPriority, message);

            AssertThatLogWasSaved(message);
        }

        private const string Tag = "TEST";

        private static void AssertThatLogWasSaved(string message)
        {
            Log.Flush();
            Assert.True(TestLogWriter.Messages.ToString().Contains(message));
        }
    }
}