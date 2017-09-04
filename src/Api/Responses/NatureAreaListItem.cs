using System;
using System.Collections.Generic;
using Types;

namespace Api.Responses
{
    public class NatureAreaListItem
    {
        public Guid LocalId { get; set; }
        public string NatureLevelCode { get; set; }
        public string NatureLevelDescription { get; set; }
        public string NatureLevelUrl { get; set; }
        public List<Parameter> Parameters { get; set; }
        public string SurveyScale { get; set; }
        public int? SurveyedYear { get; set; }
        public string Contractor { get; set; }
        public string Surveyer { get; set; }
        public string Program { get; set; }
    }
}
