using System.Collections.Generic;
using Newtonsoft.Json;
using Nin.Diagnostic;
using Types;

namespace Nin.Naturtyper
{
    public class Naturetypekodetre
    {
        readonly Dictionary<string, KodeInstans> koder = new Dictionary<string, KodeInstans>();
        public int Count => koder.Count;
        public object Koder => koder;

        public CodeItem HentFraKode(string code)
        {
            code = code.Replace("_", " ");
            var codeItem = GetCodeItem(code, out string parentCodeItemId);
            var parentCodeItems = new Stack<CodeItem>();

            while (!string.IsNullOrEmpty(parentCodeItemId))
            {
                var parentCodeItem = GetCodeItem(parentCodeItemId, out parentCodeItemId);
                parentCodeItems.Push(parentCodeItem);
            }

            while (parentCodeItems.Count > 0)
                codeItem.ParentCodeItems.Add(parentCodeItems.Pop());

            return codeItem;
        }

        public CodeItem HentFraKode(NatureLevel natureLevel)
        {
            return HentFraKode(Naturnivå.TilKode(natureLevel));
        }

        private CodeItem GetCodeItem(string kode, out string parentCodeItemId)
        {
            if (!koder.ContainsKey(kode))
            {
                var missing = new CodeItem
                {
                    Id = kode,
                    Name = "?",
                    Url = "?"
                };
                parentCodeItemId = "";
                Log.w("CODE", "Mangler nøkkel '" + missing.Id + "' i kodetre.");
                return missing;
            }
            KodeInstans kodeInstans = koder[kode];
            var codeItem = new CodeItem
            {
                Id = kodeInstans.Kode.Id,
                Name = kodeInstans.Navn,
                Url = kodeInstans.Beskrivelse
            };
            parentCodeItemId = kodeInstans.OverordnetKode.Id;

            return codeItem;
        }

        public void Add(string kodeId, KodeInstans kodeInstans)
        {
            koder.Add(kodeInstans.Kode.Id, kodeInstans);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(koder);
        }
    }

    public class KodeInstans
    {
        public string Navn { get; set; }
        public Kode Kode { get; set; }
        public Kode OverordnetKode { get; set; }
        public string Beskrivelse { get; set; }
    }

    public class Kode
    {
        public string Id { get; set; }
    }
}