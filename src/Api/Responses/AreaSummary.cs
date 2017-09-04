using System.Collections.Generic;

namespace Nin.Api.Responses
{
    public class AreaSummary
    {
        public Dictionary<int, AreaSummaryItem> Areas { get; set; }
        public Dictionary<string, AreaSummaryItem> ConservationAreas { get; set; }
        public int AreaCount { get; set; }
        public int ConservationAreaCount { get; set; }

        public AreaSummary()
        {
            Areas = new Dictionary<int, AreaSummaryItem>();
            ConservationAreas = new Dictionary<string, AreaSummaryItem>();
        }
    }
}
