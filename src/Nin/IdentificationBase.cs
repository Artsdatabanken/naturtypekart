using System;

namespace Nin.Types
{
    public abstract class IdentificationBase
    {
        protected IdentificationBase() { }
        protected IdentificationBase(IdentificationBase identification)
        {
            LocalId = identification.LocalId;
            NameSpace = identification.NameSpace;
            VersionId = identification.VersionId;
        }

        public Guid LocalId { get; set; }
        public string NameSpace { get; set; }
        public string VersionId { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class Identification : Types.IdentificationBase
    {
        public Identification() { }
        public Identification(MsSql.Identification identification) : base(identification) { }
    }
}

namespace Nin.Types.MsSql
{
    public class Identification : Types.IdentificationBase
    {
        public Identification() { }
        public Identification(RavenDb.Identification identification) : base(identification) { }
    }
}