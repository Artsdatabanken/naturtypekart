using System;

namespace Types
{
    public abstract class Parameter
    {
        protected Parameter() { }
        protected Parameter(Parameter parameter)
        {
            Code = parameter.Code;
            Surveyed = parameter.Surveyed;
        }

        public string Code { get; set; }
        public DateTime? Surveyed { get; set; }

        // *** Extra attributes (only used during exporting list content) ***
        public string CodeDescription { get; set; }
        public string CodeUrl { get; set; }
        public string MainTypeCode { get; set; }
        public string MainTypeDescription { get; set; }
        public string MainTypeCodeUrl { get; set; }
        // ******************************************************************
    }
}
