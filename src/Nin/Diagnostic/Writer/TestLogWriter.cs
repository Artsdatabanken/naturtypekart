using System.Diagnostics;
using System.Text;

namespace Nin.Common.Diagnostic.Writer
{
    public class TestLogWriter : UnbufferedLogWriter
    {
        public static StringBuilder Messages = new StringBuilder();

        public override void Write(LogEntry entry)
        {
            Messages.AppendLine(entry.ToString());
            Debug.WriteLine(entry.ToString());
        }
    }
}