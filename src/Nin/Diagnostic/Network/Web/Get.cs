using System.Net;
using System.Threading.Tasks;

namespace Common.Diagnostic.Network.Web
{
    public class Get : Http
    {
        protected override string Method => "GET";

        protected override Task<string> MakeTheCall()
        {
            return new WebClient().DownloadStringTaskAsync(Url);
        }

        public Get(string url) : base(url)
        {
        }
    }
}