using System.IO;

namespace Nin.Types.RavenDb
{
    public class File
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Stream Content { get; set; }
    }
}
