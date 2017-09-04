using System.Collections.Generic;

namespace Nin.Api.Responses
{
    public class CodeSummaryItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int Count { get; set; }
        public Dictionary<string, CodeSummaryItem> Codes { get; set; }
        public HashSet<int> HandledIds { get; set; }

        public CodeSummaryItem()
        {
            Codes = new Dictionary<string, CodeSummaryItem>();
            HandledIds = new HashSet<int>();
        }

        public CodeSummaryItem(string name, string url, int count)
        {
            Name = name;
            Url = url;
            Count = count;
            Codes = new Dictionary<string, CodeSummaryItem>();
            HandledIds = new HashSet<int>();
        }
    }
}
