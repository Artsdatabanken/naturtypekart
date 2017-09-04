using System;
using System.Text.Encodings.Web;
using Nin.Common;

namespace Common.Diagnostic
{
    public class HtmlLogReport : LogReport
    {
        private readonly HtmlEncoder htmlEncoder;

        public HtmlLogReport()
        {
            var settings = new TextEncoderSettings();
            htmlEncoder = HtmlEncoder.Create(settings);
        }

        public override void WriteLine(LogPriority logPriority, string[] msg)
        {
            string color = GetColor(logPriority);
            sb.Append("<tr style=\"vertical-align: top\">");
            foreach (var part in msg)
            {
                string encoded = htmlEncoder.Encode(part);
                var boldedMessage = "<pre>" + encoded + "</pre>";
                sb.AppendLine($"<td><div style=\"color: {color}\">{boldedMessage}</div></td>");
            }
            sb.Append("</tr>");
        }

        public override void Start()
        {
            sb.AppendLine("<table>");
        }

        public override void End()
        {
            sb.AppendLine("</table>");
        }

        private string GetColor(LogPriority logPriority)
        {
            switch (logPriority)
            {
                case LogPriority.Verbose:
                    return "black";
                case LogPriority.Debug:
                    return "darkgrey";
                case LogPriority.Info:
                    return "darkgreen";
                case LogPriority.Warn:
                    return "orange";
                case LogPriority.Error:
                    return "red";
                case LogPriority.Assert:
                    return "darkviolet";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logPriority), logPriority, null);
            }
        }
    }
}