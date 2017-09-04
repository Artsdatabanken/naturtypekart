using Newtonsoft.Json;
using Nin.Dataleveranser;

namespace Nin.Types
{
    public abstract class CustomVariableDefinition : NinVariabelDefinisjon
    {
        public CustomVariableDefinition() {}
        protected CustomVariableDefinition(CustomVariableDefinition customVariableDefinition)
        {
            Specification = customVariableDefinition.Specification;
            Description = customVariableDefinition.Description;
        }

        public string Specification { get; set; }
        public string Description { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class CustomVariableDefinition : Types.CustomVariableDefinition
    {
        public CustomVariableDefinition() {}
        public CustomVariableDefinition(MsSql.CustomVariableDefinition customVariableDefinition) : base(customVariableDefinition) {}
    }
}

namespace Nin.Types.MsSql
{
    public class CustomVariableDefinition : Types.CustomVariableDefinition
    {
        public CustomVariableDefinition() {}
        public CustomVariableDefinition(RavenDb.CustomVariableDefinition customVariableDefinition) : base(customVariableDefinition) {}

        [JsonIgnore]
        public int Id { get; set; }
    }
}
