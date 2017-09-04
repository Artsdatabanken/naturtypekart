using System;
using Newtonsoft.Json;

namespace Nin.Types
{
    public abstract class DocumentBase
    {
        protected DocumentBase()
        {
            Guid = Guid.NewGuid();
        }
        protected DocumentBase(DocumentBase document)
        {
            Guid = document.Guid;
            Title = document.Title;
            Description = document.Description;
            FileName = document.FileName;
        }

        public Guid Guid { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class Document : Types.DocumentBase
    {
        public Document() { }

        public Document(MsSql.Document document) : base(document)
        {
            if (document.Author != null) Author = new Contact(document.Author);
        }

        public Contact Author { get; set; }
    }
}

namespace Nin.Types.MsSql
{
    public class Document : Types.DocumentBase
    {
        public Document() { }

        public Document(RavenDb.Document document) : base(document)
        {
            if (document.Author != null) Author = new Contact(document.Author);
        }

        [JsonIgnore]
        public int Id { get; set; }

        public Contact Author { get; set; }
    }
}
