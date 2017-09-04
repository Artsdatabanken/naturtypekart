
using System.Collections.ObjectModel;

namespace Nin.Api.Responses
{
    public class NatureAreaStatistics
    {
        public CodeSummaryItem NatureAreaTypes { get; set; }
        public Collection<NatureAreaSummaryItem> Institutions { get; set; }
        public Collection<NatureAreaSurveyYearSummaryItem> SurveyYears { get; set; }  

        public NatureAreaStatistics()
        {
            Institutions = new Collection<NatureAreaSummaryItem>();
            SurveyYears = new Collection<NatureAreaSurveyYearSummaryItem>();
        }
    }
}
