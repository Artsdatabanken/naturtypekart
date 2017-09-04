using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Nin.Diagnostic;

namespace Nin.Configuration
{
    /// <summary>
    /// JSON Config, mellomsteg
    /// </summary>
    public class JConfig : JObject
    {
        public JConfig()
        {
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Log.i("CFG", "ASPNETCORE_ENVIRONMENT: " + environment);
        }

        public void MergeConfig(string directory)
        {
            //Log.d("CFG", $"Merge from '{directory}'");
            string parentDirectory = System.IO.Path.GetDirectoryName(directory);
            if (parentDirectory != null)
                MergeConfig(parentDirectory);

            MergeConfigFrom(System.IO.Path.Combine(directory, $"{Config.ApplicationName}.json"));

            if (!string.IsNullOrEmpty(environment))
                MergeConfigFrom(System.IO.Path.Combine(directory, $"{Config.ApplicationName}.{environment}.json"));
        }

        private void MergeConfigFrom(string filePath)
        {
            if (!File.Exists(filePath))
                return;
            Log.i("CFG", "Reading config from file '" + filePath + "'...");
            string json = File.ReadAllText(filePath);
            JObject overrides = Parse(json);
            Merge(overrides, MergeSettings);
            foundConfig = true;
        }

        private static readonly JsonMergeSettings MergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
        private readonly string environment;
        public bool foundConfig;
    }
}