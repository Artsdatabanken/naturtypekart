using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Types;

namespace Nin.IO
{
    public class SearchFilterRequest
    {
        public Collection<string> NatureLevelCodes { get; set; } = new BindingList<string>();
        public Collection<string> NatureAreaTypeCodes { get; set; } = new BindingList<string>();
        public Collection<string> DescriptionVariableCodes { get; set; } = new BindingList<string>();
        public Collection<int> Municipalities { get; set; } = new BindingList<int>();
        public Collection<int> Counties { get; set; } = new BindingList<int>();
        public Collection<int> ConservationAreas { get; set; } = new BindingList<int>();
        public Collection<string> Institutions { get; set; }  = new BindingList<string>();

        public string Geometry { get; set; }
        public string BoundingBox { get; set; }
        public int EpsgCode { get; set; }

        public bool CenterPoints { get; set; }
        public int IndexFrom { get; set; }
        public int IndexTo { get; set; }

        public Collection<NatureLevel> AnalyzeSearchFilterRequest()
        {
            DescriptionVariableCodes = FilterOutBeSysForSomeOddReason(DescriptionVariableCodes);

            var natureLevels = GetNatureLevels(NatureLevelCodes);
            return natureLevels;
        }

        private static Collection<string> FilterOutBeSysForSomeOddReason(IEnumerable<string> descriptionVariableCodes)
        {
            Collection<string> filtered = new Collection<string>();
            foreach (var descriptionVariableCode in descriptionVariableCodes)
                if (!descriptionVariableCode.StartsWith("BeSys"))
                    filtered.Add(descriptionVariableCode);

            return filtered;
        }

        private static NatureLevel GetNatureLevel(string natureLevelCode)
        {
            switch (natureLevelCode)
            {
                case "NA":
                    return NatureLevel.Natursystem;
                case "LA":
                    return NatureLevel.Landskapstype;
                case "LD":
                    return NatureLevel.Naturkompleks;
                case "LI":
                    return NatureLevel.Livsmedium;
                case "NK":
                    return NatureLevel.Landskapsdel;
                case "X":
                    return NatureLevel.Naturkomponent;
                case "EO":
                    return NatureLevel.KnowledgeArea;
                default:
                    throw new Exception("Ukjent naturnivå '"+natureLevelCode+"'.");
            }
        }

        private static Collection<NatureLevel> GetNatureLevels(Collection<string> natureLevelCodes)
        {
            if (natureLevelCodes == null) return null;
            var natureLevels = new Collection<NatureLevel>();
            foreach (var natureLevelCode in natureLevelCodes)
                natureLevels.Add(GetNatureLevel(natureLevelCode));
            return natureLevels;
        }
    }
}
