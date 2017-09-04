using System.Collections.Generic;

namespace Nin.Områder
{
    public class Områdetyper
    {
        private static readonly Dictionary<string, Områdetype> typer = new Dictionary<string, Områdetype>();

        static Områdetyper()
        {
            Add("NR", "Naturreservat");
            Add("NP", "Nasjonalpark");
            Add("LVO", "Landskapsvernområde");
            Add("D", "Dyrelivsfredning");
            Add("PD", "Plante- og dyrelivsfredning");
            Add("NM", "Naturminne");
            Add("LVOP", "Landskapsvernområde med plantelivsfredning");
            Add("DO", "Dyrefredningsområde");
            Add("LVOD", "Landskapsvernområde med dyrelivsfredning");
            Add("PO", "Plantefredningsområde");
            Add("LVOPD", "Landskapsvernområde med plante- og dyrelivsfredning");
            Add("PDO", "Plante- og dyrefredningsområde");
            Add("MIV", "Midlertidig verna område/objekt");
            Add("P", "Plantelivsfredning");
            Add("BVV", "Biotopvern etter viltloven");
            Add("NRS", "Naturreservat (Svalbardmiljøloven)");
            Add("NPS", "Nasjonalpark (Svalbardmiljøloven)");
            Add("GVS", "Geotopvern (Svalbardmiljøloven)");
            Add("BV", "Biotopvern");
            Add("MAV", "Marint verneområde (naturmangfoldloven)");
        }

        private static void Add(string kode, string navn)
        {
            typer.Add(kode, new Områdetype(kode, navn));
        }

        public static string KodeTilNavn(string kode)
        {
            if (!typer.ContainsKey(kode))
                return $"Andre ({kode})";
            return typer[kode].Navn;
        }
    }
}