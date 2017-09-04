using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using Nin.Diagnostic;

namespace Nin.Configuration
{
    /// <summary>
    /// Override configuration settings using environment variables.
    /// i.e. environment variable 'Namespace.Nin' maps to Config.Settings.Namespace.Nin.
    /// </summary>
    public static class EnvironmentVariables
    {
        public static void AddOverridesTo(JObject config)
        {
            SetVariables(config, EnvironmentVariableTarget.Machine);
            SetVariables(config, EnvironmentVariableTarget.Process);
            SetVariables(config, EnvironmentVariableTarget.User);
        }

        private static void SetVariables(JObject j, EnvironmentVariableTarget environmentVariableTarget)
        {
            var vars = Environment.GetEnvironmentVariables(environmentVariableTarget);
            foreach (DictionaryEntry kv in vars)
                SetVariable(j, (string)kv.Key, (string)kv.Value);
        }

        private static void SetVariable(JObject j, string key, string value)
        {
            key = key.Replace("_", ".");
            key = key.Replace(" ", "_");
            key = key.Replace("(", "_");
            key = key.Replace(")", "_");
            try
            {
                JToken n = j.SelectToken(key);
                if (!(n is JValue)) return;
                Log.d("CFG", $"Setting {key}={value} from environment variable.  Was: {n}");
                var prop = (JValue)n;
                prop.Value = value;
            }
            catch (Exception caught)
            {
                throw new Exception($"Invalid environment variable '{key}'.", caught);
            }
        }
    }
}