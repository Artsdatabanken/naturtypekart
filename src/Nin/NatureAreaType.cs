using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Types;

namespace Nin.Types
{
    public abstract class NatureAreaType : Parameter
    {
        protected NatureAreaType() { }
        protected NatureAreaType(NatureAreaType natureAreaType) : base(natureAreaType)
        {
            Share = natureAreaType.Share;
        }

        public double Share { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class NatureAreaType : Types.NatureAreaType
    {
        public NatureAreaType() { }

        public NatureAreaType(MsSql.NatureAreaType natureAreaType) : base(natureAreaType)
        {
            if (natureAreaType.Surveyer != null) Surveyer = new Contact(natureAreaType.Surveyer);

            foreach (var customVariable in natureAreaType.CustomVariables)
            {
                CustomVariables.Add(new CustomVariable(customVariable));
            }
            foreach (var additionalVariable in natureAreaType.AdditionalVariables)
            {
                AdditionalVariables.Add(new DescriptionVariable(additionalVariable));
            }
        }

        public Contact Surveyer { get; set; }

        public Collection<CustomVariable> CustomVariables => customVariables ?? (customVariables = new Collection<CustomVariable>());
        public Collection<DescriptionVariable> AdditionalVariables => additionalVariables ?? (additionalVariables = new Collection<DescriptionVariable>());

        private Collection<CustomVariable> customVariables;
        private Collection<DescriptionVariable> additionalVariables;
    }
}

namespace Nin.Types.MsSql
{
    public class NatureAreaType : Types.NatureAreaType
    {
        public NatureAreaType() { }

        public NatureAreaType(RavenDb.NatureAreaType natureAreaType) : base(natureAreaType)
        {
            if (natureAreaType.Surveyer != null) Surveyer = new Contact(natureAreaType.Surveyer);

            foreach (var customVariable in natureAreaType.CustomVariables)
                CustomVariables.Add(new CustomVariable(customVariable));
            foreach (var additionalVariable in natureAreaType.AdditionalVariables)
                AdditionalVariables.Add(new DescriptionVariable(additionalVariable));
        }

        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public int NatureAreaId { get; set; }
        public Contact Surveyer { get; set; }

        private Collection<CustomVariable> customVariables;
        public Collection<CustomVariable> CustomVariables
        {
            get => customVariables ?? (customVariables = new Collection<CustomVariable>());
            set => customVariables = value;
        }

        private Collection<DescriptionVariable> additionalVariables;
        public Collection<DescriptionVariable> AdditionalVariables
        {
            get => additionalVariables ?? (additionalVariables = new Collection<DescriptionVariable>());
            set => additionalVariables = value;
        }
    }
}
