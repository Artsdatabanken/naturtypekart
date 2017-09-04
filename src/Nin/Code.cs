namespace Nin.Types
{
    public abstract class Code
    {
        protected Code() {}
        protected Code(Code code)
        {
            Registry = code.Registry;
            Version = code.Version;
            Value = code.Value;
        }

        public string Registry { get; set; }
        public string Version { get; set; }
        public string Value { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class Code : Types.Code
    {
        public Code() {}
        public Code(MsSql.Code code) : base (code) {}
    }
}

namespace Nin.Types.MsSql
{
    public class Code : Types.Code
    {
        public Code() {}
        public Code(RavenDb.Code code) : base (code) {}
    }
}