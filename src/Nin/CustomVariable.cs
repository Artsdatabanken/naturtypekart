using Newtonsoft.Json;

namespace Nin.Types
{
    public abstract class CustomVariable
    {
        protected CustomVariable() {}
        protected CustomVariable(CustomVariable customVariable)
        {
            Specification = customVariable.Specification;
            Value = customVariable.Value;
        }

        public string Specification { get; set; }
        public string Value { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class CustomVariable : Types.CustomVariable
    {
        public CustomVariable() {}
        public CustomVariable(MsSql.CustomVariable customVariable) : base(customVariable) {}
    }
}

namespace Nin.Types.MsSql
{
    public class CustomVariable : Types.CustomVariable
    {
        public CustomVariable() {}
        public CustomVariable(RavenDb.CustomVariable customVariable) : base(customVariable) {}
        public CustomVariable(CustomVariable customVariable) : base(customVariable) {}

        [JsonIgnore]
        public int Id { get; set; }
    }

    public class CustomVariableExport : CustomVariable
    {
        public CustomVariableExport(CustomVariable customVariable) : base(customVariable) {}

        public string Description { get; set; }
    }
}