using System;
using Nin.Områder;

namespace Nin.Map.Tiles.Geometri
{
    [Serializable]
    public class Område
    {
        public int AreaId;
        public int Number;
        public string Category;
        public string Name;
        public string kind;
        public AreaType Type;
        public string Value;

        public Område(int areaId, AreaType areaType)
        {
            AreaId = areaId;
            Type = areaType;
        }

        public override string ToString()
        {
            return $"{Type} #{AreaId}: {Name}";
        }

        public static Område Fra(Area area)
        {
            Område område = new Område(area.Id, area.Type)
            {
                Number = area.Number,
                Category = area.Category,
                Name = area.Name,
                kind = Map(area.Type, area.Category),
                Value = area.Value
            };
            return område;
        }

        private static string Map(AreaType t, string category)
        {
            switch (t)
            {
                case AreaType.Grid:
                    return "rutenett";
                case AreaType.Land:
                    return "land";
                case AreaType.Kommune:
                    return "kommune";
                case AreaType.Fylke:
                    return "fylke";
                case AreaType.Naturområde:
                case AreaType.Verneområde:
                    return category;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }
    }
}