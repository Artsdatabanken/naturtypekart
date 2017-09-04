using Nin.Common;
using Nin.Naturtyper;

namespace Nin.Tasks
{
    public class RefreshCodeTreeTask : Task
    {
        public KodetreType KodetreType;

        public override void Execute(NinServiceContext context)
        {
            Naturkodetrær.Refresh(KodetreType);
        }
    }
}