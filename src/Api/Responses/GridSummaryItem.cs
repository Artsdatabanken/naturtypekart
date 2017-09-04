using System.Collections.ObjectModel;
using Nin.Api.Responses;
using Nin.Dataleveranser.Rutenett;
using Nin.Områder;

namespace Api.Responses
{
    public class GridSummaryItem
    {
        public GridSummaryItem(RutenettType rutenettType)
        {
            GridType = rutenettType.ToString();
            GridDescription = GetDescription(rutenettType);
            GridLayers = new Collection<GridLayerSummaryItem>();
        }

        public GridSummaryItem(AreaType areaType)
        {
            GridType = areaType.ToString();
            GridDescription = GetDescription(areaType);
            GridLayers = new Collection<GridLayerSummaryItem>();
        }

        public string GridType { get; }
        public string GridDescription { get; }
        public Collection<GridLayerSummaryItem> GridLayers { get; }

        private static string GetDescription(RutenettType rutenettType)
        {
            switch (rutenettType)
            {
                case RutenettType.SSB0250M:
                    return "SSB 250m";
                case RutenettType.SSB0500M:
                    return "SSB 500m";
                case RutenettType.SSB001KM:
                    return "SSB 1km";
                case RutenettType.SSB002KM:
                    return "SSB 2km";
                case RutenettType.SSB005KM:
                    return "SSB 5km";
                case RutenettType.SSB010KM:
                    return "SSB 10km";
                case RutenettType.SSB025KM:
                    return "SSB 25km";
                case RutenettType.SSB050KM:
                    return "SSB 50km";
                case RutenettType.SSB100KM:
                    return "SSB 100km";
                case RutenettType.SSB250KM:
                    return "SSB 250km";
                case RutenettType.SSB500KM:
                    return "SSB 500km";
                default:
                    return "Andre";
            }
        }

        private static string GetDescription(AreaType areaType)
        {
            switch (areaType)
            {
                case AreaType.Kommune:
                    return "Kommune";
                case AreaType.Fylke:
                    return "Fylke";
                default:
                    return "Andre områder";
            }
        }
    }
}