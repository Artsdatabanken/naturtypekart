namespace Nin.Types
{
    public abstract class QualityBase
    {
        protected QualityBase() { }
        protected QualityBase(QualityBase quality)
        {
            MeasuringMethod = quality.MeasuringMethod;
            Accuracy = quality.Accuracy;
            Visibility = quality.Visibility;
            MeasuringMethodHeight = quality.MeasuringMethodHeight;
            AccuracyHeight = quality.AccuracyHeight;
            MaxDeviation = quality.MaxDeviation;
        }

        public string MeasuringMethod { get; set; } // Should we store codelist value or key here?
        public int? Accuracy { get; set; }
        public string Visibility { get; set; } // Should we store codelist value or key here?
        public string MeasuringMethodHeight { get; set; } // Should we store codelist value or key here?
        public int? AccuracyHeight { get; set; }
        public int? MaxDeviation { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class Quality : Types.QualityBase
    {
        public Quality() { }
        public Quality(MsSql.Quality quality) : base(quality) { }
    }
}

namespace Nin.Types.MsSql
{
    public class Quality : Types.QualityBase
    {
        public Quality() { }
        public Quality(RavenDb.Quality quality) : base(quality) { }
    }
}
