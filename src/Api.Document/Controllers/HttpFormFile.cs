using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Nin.Dataleveranser.Import;

namespace Api.Document.Controllers
{
    /// <summary>
    /// Deals with files uploaded via Web forms.
    /// </summary>
    public class HttpFormFile : IDataFile
    {
        public override string ContentType => formFile.ContentType;

        public override string Filename
        {
            get
            {
                var dispositionHeaderValue = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition);
                var filePath = dispositionHeaderValue.FileName.Trim('"');
                return Path.GetFileName(filePath);
            }
        }

        public override Stream OpenReadStream()
        {
            return formFile.OpenReadStream();
        }

        public HttpFormFile(IFormFile src)
        {
            formFile = src ?? throw new ArgumentNullException(nameof(src));
        }

        private readonly IFormFile formFile;

        public static DataFiles GetFormFiles(IFormFileCollection formFileCollection)
        {
            var files = new DataFiles();
            for (int i = 0; i < formFileCollection.Count - 1; i++)
            {
                var name = "files" + i;
                var formFile = formFileCollection.GetFile(name);
                if (formFile == null)
                    throw new Exception("Mangler fil med navn '" + name + "'.");

                var file = new HttpFormFile(formFile);
                files.Add(file.Filename, file);
            }
            return files;
        }
    }
}