using System.Collections.Generic;

namespace Nin.Api.Responses
{
    public class AreaSummaryItem
    {
        public string Name { get; set; }
        public int NatureAreaCount { get; set; }
        public Dictionary<int, AreaSummaryItem> Areas { get; set; }

        public AreaSummaryItem(string name, int natureAreaCount)
        {
            Name = name;
            NatureAreaCount = natureAreaCount;
            Areas = new Dictionary<int, AreaSummaryItem>();
        }
    }
}
