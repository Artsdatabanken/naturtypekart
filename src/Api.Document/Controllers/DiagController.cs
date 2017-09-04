using System;
using System.Data.SqlClient;
using Common.Diagnostic;
using Microsoft.AspNetCore.Mvc;
using Nin.Aspnet;
using Nin.Common;
using Nin.Common.Diagnostic;
using Nin.Diagnostic;
using Nin.Diagnostic.Writer;
using Nin.IO.SqlServer;

namespace Api.Document.Controllers
{
    public class DiagController : Controller
    {
        //public DiagModule()
        //{
        //    Get("/log/{priority?:string}", parameters => DumpLog(parameters.priority));
        //    Get("/tasklog", _ => TaskLog());
        //    Get("/taskqueue", _ => TaskQueue());
        //    Get("/{count?:int}", parameters => Index(parameters.count));
        //}

        [HttpGet]
        public IActionResult Index(int maxCount = 200)
        {
            var html = new HtmlWriter();
            html.Append("<pre>");

            html.Append(QueryToHtml("TaskQueueError", maxCount));
            html.Append(QueryToHtml("TaskQueue", maxCount));
            html.Append(QueryToHtml("TaskLog", maxCount));

            html.Tag("h3", "SysLog Verbose");
            html.Append(SysLogToHtml("Verbose", maxCount));

            html.Append("</pre>");
            var contentResult = new NinHtmlResult(html.ToString());
            Log.v("DIAG", "Index: " + contentResult.Content.Length);
            return contentResult;
        }

        [HttpGet]
        public IActionResult DumpLog(string priority = "Verbose")
        {
            var r = SysLogToHtml(priority);
            var contentResult = new NinHtmlResult("<pre>" + r + "</pre>");
            Log.v("DIAG", "Log: " + contentResult.Content.Length);
            return contentResult;
        }

        [HttpGet]
        public IActionResult TaskQueue()
        {
            var html = QueryToHtml("TaskQueue");
            var contentResult = new NinJsonResult(html);
            Log.v("DIAG", "TaskQueue: " + contentResult.Content.Length);
            return contentResult;
        }

        [HttpGet]
        public ContentResult TaskLog()
        {
            var html = QueryToHtml("TaskLog");
            var contentResult = new NinJsonResult(html);
            Log.v("DIAG", "TaskLog: " + contentResult.Content.Length);
            return contentResult;
        }

        private static string SysLogToHtml(string priority, int maxCount = 1000)
        {
            LogPriority logPriority = (LogPriority)Enum.Parse(typeof(LogPriority), priority);
            var r = SyslogTableLogWriter.Dump(logPriority, new HtmlLogReport(), maxCount);
            return r;
        }

        private static string QueryToHtml(string tableName, int maxCount = 1000)
        {
            var rows = new HtmlWriter();
            using (SqlStatement cmd = SqlServer.Query(tableName, maxCount))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.HasRows)
                {
                    rows.Tag("h3", tableName + ": Empty");
                    return rows.ToString();
                }

                var headers = new HtmlWriter();
                for (int i = 0; i < r.FieldCount; i++)
                    headers.Tag("th", r.GetName(i));
                rows.TagRaw("tr", headers.ToString());

                while (r.Read())
                {
                    var row = new HtmlWriter();
                    for (int i = 0; i < r.FieldCount; i++)
                        row.Tag("TD", r[i].ToString().Replace("\r\n", "<br />"));
                    rows.TagRaw("tr", row.ToString());
                }
            }

            var html = new HtmlWriter();
            int count = SqlServer.QueryRecordCount(tableName);
            html.Tag("h3", tableName + " (rows 1-" + Math.Min(maxCount, count) + " of " + count + ")");
            html.TagRaw("table", rows.ToString());
            return html.ToString();
        }
    }
}
