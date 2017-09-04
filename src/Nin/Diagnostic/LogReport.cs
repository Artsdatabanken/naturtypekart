using System.Text;
using Nin.Common;

namespace Common.Diagnostic
{
    public abstract class LogReport
    {
        protected readonly StringBuilder sb = new StringBuilder();
        public abstract void WriteLine(LogPriority logPriority, string[] msg);

        public string GetOutput()
        {
            return sb.ToString();
        }

        public abstract void Start();
        public abstract void End();
    }
}