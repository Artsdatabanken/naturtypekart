using System.Text;
using System.Web;

namespace Nin.Common.Diagnostic
{
    public class HtmlWriter
    {
        readonly StringBuilder html = new StringBuilder();

        public void Tag(string tag, string innerHtml)
        {
            html.AppendLine($"<{tag}>{HttpUtility.HtmlEncode(innerHtml)}</{tag}>");
        }

        public void TagRaw(string tag, string innerHtml)
        {
            html.AppendLine($"<{tag}>{innerHtml}</{tag}>");
        }

        public override string ToString()
        {
            return html.ToString();
        }

        public void Append(string html)
        {
            this.html.AppendLine(html);
        }
    }
}