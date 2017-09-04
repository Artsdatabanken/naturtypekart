using System;

namespace Types
{
    public enum NatureLevel
    {
        /// <summary>
        /// Landskapstype er et større geografisk område med enhetlig visuelt preg, skapt av enhetlig dominans av store landformer og kjennetegnet ved karakteristisk fordeling av landformer, naturkomplekser, natursystemer og andre landskapselementer.
        /// Kode: LA
        /// http://artsdatabanken.no/Pages/182858
        /// </summary>
        Landskapstype = 1,

        Landskapsdel = 2,

        /// <summary>
        /// Natur(system)kompleks er et kompleks av natursystemer som i naturen utgjør en funksjonell økologisk, eventuelt også geomorfologisk, enhet, og som forekommer innenfor et velavgrenset geografisk område
        /// http://artsdatabanken.no/Pages/182859
        /// Kode: NK
        /// http://artsdatabanken.no/Pages/182859
        /// </summary>
        Naturkompleks = 3,

        /// <summary>
        /// Natursystem defineres av «alle organismer innen et mer eller mindre enhetlig, avgrensbart område, det totale miljøet de lever i og er tilpasset til, og de prosesser som regulerer relasjoner organismene imellom og mellom organismer og miljø (herunder menneskelig aktivitet)»
        /// Kode: NA
        /// http://artsdatabanken.no/Pages/222921
        /// </summary>
        Natursystem = 4,

        /// <summary>
        /// Natur(system)komponent er en geografisk velavgrenset, funksjonell økologisk enhet som tilfredsstiller definisjonen av natursystem, men som utgjør én (vanligvis blant flere) komponenter i et natursystem.
        /// http://artsdatabanken.no/Pages/180518
        /// </summary>
        Naturkomponent = 5,

        /// <summary>
        /// Inndelingen på livsmedium-nivået skal gi oss begreper for å karakterisere individers og arters levebetingelser. Livsmedium-inndelingen omfatter bunn, mark, vannmasser og luft og er fullstendig dekkende for det norske fastlandet samt for havområdene og arktiske øyer under norsk suverenitet.
        /// Kode: LI
        /// http://artsdatabanken.no/Pages/137826
        /// </summary>
        Livsmedium = 6,

        /// <summary>
        /// WTF?
        /// </summary>
        KnowledgeArea = 7
    }

    public static class Naturnivå
    {
        // TODO: Correct this functions when correct codes appear in the nin code v2.0 web service.
        public static string TilKode(NatureLevel natureLevel)
        {
            switch (natureLevel)
            {
                case NatureLevel.Natursystem:
                    return "NA";
                case NatureLevel.Landskapstype:
                    return "LA";
                case NatureLevel.Naturkompleks:
                    return "LD";
                case NatureLevel.Livsmedium:
                    return "LI";
                case NatureLevel.Landskapsdel:
                    return "NK";
                case NatureLevel.Naturkomponent:
                    return "X";
                case NatureLevel.KnowledgeArea:
                    return "EO";
                default:
//                    Log.e("NIN", $"Ukjent naturnivå \'{natureLevel}\'.");
                    throw new Exception($"Ukjent naturnivå \'{natureLevel}\'.");
            }
        }

        public static string TilNavn(NatureLevel natureLevel)
        {
            switch (natureLevel)
            {
                case NatureLevel.Natursystem:
                    return "Natursystem";
                case NatureLevel.Landskapstype:
                    return "Landskapstype";
                case NatureLevel.Naturkompleks:
                    return "Naturkompleks";
                case NatureLevel.Livsmedium:
                    return "Livsmedium";
                case NatureLevel.Landskapsdel:
                    return "Landskapsdel";
                case NatureLevel.Naturkomponent:
                    return "Naturkomponent";
                case NatureLevel.KnowledgeArea:
                    return "Egenskapsområde";
                default:
                    throw new Exception("ERROR: Unknown nature level code");
            }
        }
    }
}
