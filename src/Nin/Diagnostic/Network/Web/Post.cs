using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Diagnostic.Network.Web
{
    public class Post : Http
    {
        private readonly HttpContent content;
        protected override string Method => "POST";

        protected override Task<string> MakeTheCall()
        {
            var r = Client.PostAsync(Url, content);
            r.Wait();
            if (r.Result.StatusCode == HttpStatusCode.OK)
                return r.Result.Content.ReadAsStringAsync();

            var cr = r.Result.Content.ReadAsStringAsync();
            cr.Wait();
            if(string.IsNullOrEmpty(cr.Result))
                throw new Exception((int)r.Result.StatusCode + ": "+ r.Result.ReasonPhrase);
            throw new Exception(cr.Result);
        }

        public override void Dispose()
        {
            content.Dispose();
            base.Dispose();
        }

        public Post(string url, string body, string mediaType = "application/json") : base(url)
        {
            content = new StringContent(body, Encoding.Default, mediaType);
        }
    }
}