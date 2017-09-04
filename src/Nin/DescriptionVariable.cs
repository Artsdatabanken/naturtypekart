using Newtonsoft.Json;
using Types;

namespace Nin.Types
{
    public abstract class DescriptionVariable : Parameter
    {
        protected DescriptionVariable() {}
        protected DescriptionVariable(DescriptionVariable descriptionVariable) : base (descriptionVariable)
        {
            Value = descriptionVariable.Value;
            Description = descriptionVariable.Description;
        }

        public string Value { get; set; }
        public string Description { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class DescriptionVariable : Types.DescriptionVariable
    {
        public DescriptionVariable() {}

        public DescriptionVariable(MsSql.DescriptionVariable descriptionVariable) : base(descriptionVariable)
        {
            if (descriptionVariable.Surveyer != null) Surveyer = new Contact(descriptionVariable.Surveyer);
        }

        public Contact Surveyer { get; set; }
    }
}

namespace Nin.Types.MsSql
{
    public class DescriptionVariable : Types.DescriptionVariable
    {
        public DescriptionVariable() { }

        public DescriptionVariable(RavenDb.DescriptionVariable descriptionVariable) : base(descriptionVariable)
        {
            if (descriptionVariable.Surveyer != null) Surveyer = new Contact(descriptionVariable.Surveyer);
        }

        [JsonIgnore]
        public int Id { get; set; }
        public Contact Surveyer { get; set; }
    }
}