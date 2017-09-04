using Newtonsoft.Json;
using Nin.Dataleveranser;

namespace Nin.Types
{
    public abstract class NinStandardVariabel : NinVariabelDefinisjon
    {
    }
}

namespace Nin.Types.RavenDb
{
    public class NinStandardVariabel : Types.NinStandardVariabel
    {
        public NinStandardVariabel()
        {
        }

        public NinStandardVariabel(MsSql.NinStandardVariabel standardVariable)
        {
            if (standardVariable != null) VariableDefinition = new Code(standardVariable.VariableDefinition);
        }

        public Code VariableDefinition { get; set; }
    }
}

namespace Nin.Types.MsSql
{
    public class NinStandardVariabel : Types.NinStandardVariabel
    {
        public NinStandardVariabel()
        {
        }

        public NinStandardVariabel(RavenDb.NinStandardVariabel standardVariable) 
        {
            if (standardVariable != null) VariableDefinition = new Code(standardVariable.VariableDefinition);
        }

        [JsonIgnore]
        public int Id { get; set; }

        public Code VariableDefinition { get; set; }
    }
}