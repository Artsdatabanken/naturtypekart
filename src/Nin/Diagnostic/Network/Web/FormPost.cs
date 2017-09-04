using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Common.Diagnostic.Network.Web
{
    public class FormPost : Http
    {
        private readonly MultipartFormDataContent content;
        protected override string Method => "POST";

        protected override Task<string> MakeTheCall()
        {
            var r = Client.PostAsync(Url, content);
            r.Wait();
            if (r.Result.StatusCode != HttpStatusCode.OK)
            {
                var cr = r.Result.Content.ReadAsStringAsync();
                cr.Wait();
                throw new Exception(cr.Result);
            }
            return r.Result.Content.ReadAsStringAsync();
        }

        public override void Dispose()
        {
            content.Dispose();
            base.Dispose();
        }

        public FormPost(string url) : base(url)
        {
            content = new MultipartFormDataContent();
        }

        public void Add(string name, string value)
        {
            content.Add(new StringContent(value), name);
        }

        public void AddFile(string name, string fileName, string mediaType, string fileContents)
        {
            var fileContent = new StringContent(fileContents);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                //Name = "\"" + name + "\"",
                //FileName = "\"" + fileName + "\""
                Name = name,
                FileName = fileName 
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            content.Add(fileContent);
        }

        public void AddFile(string name, string absolutePath)
        {
            string mimeType = MimeMapping.GetMimeMapping(Path.GetExtension(absolutePath));
            AddFile(name, Path.GetFileName(absolutePath), mimeType, File.ReadAllText(absolutePath));
        }
    }
}