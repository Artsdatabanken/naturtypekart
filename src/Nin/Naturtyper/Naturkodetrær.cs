using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using Nin.Common;
using Nin.Configuration;
using Nin.Diagnostic;
using Raven.Imports.Newtonsoft.Json;

namespace Nin.Naturtyper
{
    public static class Naturkodetrær
    {
        private static Naturetypekodetre Les(KodetreType kodetreType)
        {
            var codeUrl = GetUrl(kodetreType);

            var cacheFileName = CacheFileName(kodetreType);
            if (!File.Exists(cacheFileName))
            {
                Log.d("KODE", $"Har ingen lokal kopi av kodetre på '{cacheFileName}'.");
                return RefreshCache(codeUrl, cacheFileName);
            }

            var writeTimeUtc = File.GetLastWriteTimeUtc(cacheFileName);
            var nowTimeUtc = DateTime.UtcNow;

            var fileAge = nowTimeUtc - writeTimeUtc;
            var fileAgeLimit = new TimeSpan(1, 0, 0, 0);

            if (fileAge >= fileAgeLimit)
            {
                Log.d("KODE", $"Lokal kopi av kodetre oppdatert '{writeTimeUtc}', utløp {fileAgeLimit}.");
                return RefreshCache(codeUrl, cacheFileName);
            }

            return JsonConvert.DeserializeObject<Naturetypekodetre>(File.ReadAllText(cacheFileName));
        }

        private static string GetUrl(KodetreType kodetreType)
        {
            return kodetreType == KodetreType.AlleKoder
                ? Config.Settings.ExternalDependency.NinCodeUrlAlleKoder
                : Config.Settings.ExternalDependency.NinCodeUrlVariasjon;
        }

        private static string CacheFileName(KodetreType kodetreType)
        {
            return Path.Combine(Config.Settings.CacheDirectory, kodetreType + ".json");
        }

        private static Naturetypekodetre RefreshCache(string codeUrl, string cacheFileName)
        {
            var data = DownloadString(codeUrl);

            var kodetre = new Naturetypekodetre();
            var koder = JsonConvert.DeserializeObject<Collection<KodeInstans>>(data);
            foreach (var k in koder)
                kodetre.Add(k.Kode.Id, k);

            try
            {
                UpdateCache(kodetre, cacheFileName);
            }
            catch (Exception e)
            {
                Log.e("KODE", e);
            }

            return kodetre;
        }

        private static string DownloadString(string codeUrl)
        {
            Log.d("HTTTP", "Downloading "+codeUrl);
            string data;
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                data = webClient.DownloadString(codeUrl);
            }
            return data;
        }

        private static void UpdateCache(Naturetypekodetre kodetre, string cacheFileName)
        {
            Log.d("KODE", "Oppdaterer kodetre cache på '" + cacheFileName + "'.");
            File.WriteAllText(cacheFileName, JsonConvert.SerializeObject(kodetre));
        }

        public static void Refresh(KodetreType kt)
        {
            RefreshCache(GetUrl(kt), CacheFileName(kt));
        }

        public static Naturetypekodetre Naturtyper
        {
            get
            {
                if (_alleKoder != null) return _alleKoder;

                lock (typeof(Naturkodetrær))
                    _alleKoder = Les(KodetreType.AlleKoder);
                return _alleKoder;
            }
        }

        public static Naturetypekodetre Naturvariasjon
        {
            get
            {
                if (_variasjon != null) return _variasjon;

                lock (typeof(Naturkodetrær))
                    _variasjon = Les(KodetreType.Variasjon);
                return _variasjon;
            }
        }

        private static Naturetypekodetre _alleKoder;
        private static Naturetypekodetre _variasjon;
    }
}