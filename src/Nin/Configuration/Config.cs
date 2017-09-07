using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nin.Common;
using Nin.Diagnostic;
using Nin.Map.Layers;

namespace Nin.Configuration
{
    /// <summary>
    /// Nin Configuration Settings
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Loads configuration from all sources.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static Config LoadFrom(string directory)
        {
            Log.v("CFG", $"Load from '{directory}'");
            var cfg = new JConfig();
            cfg.MergeConfig(Path.GetFullPath(directory));
            Config settings;
            if (cfg.foundConfig)
                settings = cfg.ToObject<Config>();
            else
                settings = new Config(); // Fall back to defaults

            var cfg2 = JObject.FromObject(settings);

            EnvironmentVariables.AddOverridesTo(cfg2);
            settings = cfg2.ToObject<Config>();
            if (settings.SaveResultingConfig)
                settings.SaveToFile(Path.Combine(directory, ApplicationName + ".pretty.json"));
            return settings;
        }

        public void SaveToDirectory(string directory)
        {
            var fullPath = Path.Combine(directory, ApplicationName + ".json");
            Log.i("CFG", $"Save settings to: {fullPath}");
            SaveToFile(fullPath);
        }

        private void SaveToFile(string fullPath)
        {
            var settings = new JsonSerializerSettings {Formatting = Formatting.Indented};
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            string baseJson = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(fullPath, baseJson);
        }

        public static Config Settings
        {
            get
            {
                if (_settings == null)
                    throw new Exception("Configuration settings have not been initialized.");
                return _settings;
            }
            set
            {
                _settings = value;
                Initialized?.Invoke(value, EventArgs.Empty);
            }
        }

        public static EventHandler Initialized;

        public static bool IsInitialized => _settings != null;

        public static Config Load()
        {
            var currentDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            return LoadFrom(currentDir);
        }

        public static Config LoadFromExecutablePath()
        {
            var exePath = Process.GetCurrentProcess().MainModule.FileName;
            var rootPath = Path.GetDirectoryName(exePath);
            return LoadFrom(rootPath);
        }

        public static Config _settings;

        public string ConnectionString;
        public Diagnostic Diagnostic = new Diagnostic();
        public Namespace Namespace = new Namespace();
        public ExternalDependency ExternalDependency = new ExternalDependency();
        public Proxy Proxy = new Proxy();
        public Export Export = new Export();
        public Map Map = new Map();
        public Database Database = new Database();
        public string CacheDirectory = @"cache";
        public Schedule Schedule = new Schedule();
        public bool SaveResultingConfig = false;
        public const string ApplicationName = "nin";
        public string SchemaSubdirectory = @"schema";
        public string FrontEndSubdirectory = @"web";
    }

    public class Database
    {
        public bool ImmediatelyLinkAreas = false;
    }

    public class Proxy
    {
        public string[] AllowedUrlPrefixes = {
            "http://opencache.statkart.no/",
            "http://gatekeeper1.geonorge.no/",
            "http://gatekeeper2.geonorge.no/",
            "http://gatekeeper3.geonorge.no/",
            "http://arcgisproxy.miljodirektoratet.no/",
            "http://arcgisproxy.dirnat.no/",
            "http://wms.geonorge.no/",
            "http://wms.miljodirektoratet.no/",
            "http://xkcd.com"
        };
    }

    public class Export
    {
        public int ExcelSpatialReferenceSystemIdentifier = 32633;
    }

    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class Map
    {
        public int SpatialReferenceSystemIdentifier = 32633; // 3857;
        public string MapLayersPath = @"Data\TempMap\";
        public int GeoJsonCoordinateDecimals = 2;
        public bool StoreBoundingBoxes = false;

        /// <summary>
        /// Overlap as ratio of size of overlap between vector tiles (to avoid render gaps)
        /// </summary>
        public List<TiledVectorLayer> Layers = new List<TiledVectorLayer>();

        public TiledVectorLayer FindLayer(string mapLayerName)
        {
            foreach (var layer in Layers)
                if (layer.Name == mapLayerName)
                    return layer;
            throw new Exception($"Mangler konfigurasjon for kartlag '{mapLayerName}'.");
        }
    }

    public class ExternalDependency
    {
        public DocumentArchive DocumentArchive = new DocumentArchive();
        public Email Email = new Email();
        public GeoNorge GeoNorge = new GeoNorge();

        //public string NinCodeUrl = "http://webtjenester.artsdatabanken.no/NiN/v2b/koder/alleKoder";
        public string NinCodeUrlAlleKoder = "http://webtjenester.artsdatabanken.no/NiN/v2/alleKoder";
        public string NinCodeUrlVariasjon = "http://webtjenester.artsdatabanken.no/NiN/v2b/variasjon/allekoder";

        public string UserDatabaseConnectionString =
            "Data Source=;Initial Catalog=;Persist Security Info=True;User ID=;Password=";
    }

    public class GeoNorge
    {
        public string AliasId = "";
        public string BrukerId = "";
        public string Passord = "";
        public string ProxyServerIP = "";
        public int TokenMinutesValid = 12 * 60; // minutes
    }

    public class DocumentArchive
    {
        public string DbUrl = "http://localhost:8081";
        public string FsUrl = "http://localhost:8081";
        public string DbName = "NINKart";
        public string FsName = "NINKartFs";
    }

    public class Diagnostic
    {
        public Logger[] Logging = {new Logger("Console", LogPriority.Debug)};
        public Pushover Pushover = new Pushover();
    }

    public class Logger
    {
        public string Name;
        public LogPriority MinimumPriority;

        public Logger()
        {
        }

        public Logger(string name, LogPriority minimumPriority)
        {
            Name = name;
            MinimumPriority = minimumPriority;
        }
    }

    /// <summary>
    /// Pushover real-time notifications on Android, iPhone, iPad, and Desktop (Pebble, Android Wear, and Apple watches)
    /// https://pushover.net/
    /// </summary>
    public class Pushover
    {
        public string Token = "ag6xt3ehz1iadbzwxhojo9ncunmep6";
        public string User = "uyqgsMeBmLLMrdvXM8LbeMEwThxeDv";
    }

    public class Email
    {
        public string Server = "";
        public string SenderEmail = "";
        public string SenderName = "";
    }

    public class Namespace
    {
        public string Nin => "http://pavlov.itea.ntnu.no/NbicFiles";
        public string Gml => "http://www.opengis.net/gml/3.2";
        public string Wfs => "http://www.opengis.net/wfs";
        public string Xsi => "http://www.w3.org/2001/XMLSchema-instance";
    }
}
