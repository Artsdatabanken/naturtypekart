using Nin.Områder;

namespace Nin.Tasks
{
    public class ZoomFactor
    {
        public int Minimum;
        public int Maximum;
        public AreaType AreaType;

        public ZoomFactor()
        {
        }

        public ZoomFactor(AreaType areatype, int min, int max)
        {
            AreaType = areatype;

            Minimum = min;
            Maximum = max;
        }
    }
}