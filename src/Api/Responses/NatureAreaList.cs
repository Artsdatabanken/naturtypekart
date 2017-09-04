using System.Collections.ObjectModel;

namespace Api.Responses
{
    public class NatureAreaList
    {
        public int NatureAreaCount { get; set; }
        public Collection<NatureAreaListItem> NatureAreas { get; set; }

        public NatureAreaList()
        {
            NatureAreaCount = 0;
            NatureAreas = new Collection<NatureAreaListItem>();
        }
    }
}