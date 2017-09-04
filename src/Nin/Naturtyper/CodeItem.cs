using System.Collections.ObjectModel;

namespace Nin.Naturtyper
{
    public class CodeItem 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public Collection<CodeItem> ParentCodeItems { get; set; } 

        public CodeItem()
        {
            ParentCodeItems = new Collection<CodeItem>();
        }

        public override string ToString()
        {
            var codePath = "";

            foreach (var codeItem in ParentCodeItems)
                codePath += $"{codeItem.Name} ({codeItem.Id}) -> ";

            return $"{codePath}{Name} ({Id})";
        }
    }
}
