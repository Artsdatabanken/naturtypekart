using System.Collections.Generic;
using System.Linq;

namespace Nin.Api.Responses
{
    public class CodeSummaryItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int OwnCount { get; set; }
        public int Count
        {
            get
            {
                return Codes.Sum(c => c.Value.Count) + OwnCount;
            }
        }
        
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
            OwnCount = count;
            Codes = new Dictionary<string, CodeSummaryItem>();
            HandledIds = new HashSet<int>();
        }
    }
}
