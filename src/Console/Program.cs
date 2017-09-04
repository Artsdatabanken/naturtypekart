using System;
using System.Diagnostics;
using Nin.Command;
using Nin.Common;
using Nin.Configuration;
using Nin.Diagnostic;

namespace Console
{
    /// <summary>
    /// Runs commands against the system.
    /// 
    /// USAGE:
    ///
    ///  Nin.Console server schemaname create &lt;SqlSchemaScriptFileSpec&gt;
    ///   Drops database if it exists, create a new database and runs scripts.
    ///
    ///  Nin.Console server schemaname upgrade &lt;SqlSchemaScriptFileSpec&gt;
    ///   Runs the scripts specified that have not yet been run on the database.
    ///
    ///  Nin.Console server schemaname exec &lt;SqlQuery&gt;
    ///   Executes the specified SQL statements.
    ///
    ///  Nin.Console server schemaname syslog &lt;Verbose/Debug/Info/Warn/Error/Assert&gt;
    ///   Display all syslog events or events having at least minimum priority.
    ///
    ///  Nin.Console server schemaname importarea &lt;AreaFileSpec&gt; &lt;SourceSrid&gt; &lt;AreaType&gt;
    ///   Imports the specified area data file (geojson/shp).
    ///
    ///  Nin.Console server schemaname importgrid &lt;GridFileSpec&gt;
    ///   Imports the specified grid data file.
    ///
    /// </summary>
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Config.Settings = Config.LoadFromExecutablePath();
                VerifyConnectionString();

                DatabaseCommand command = DatabaseCommand.Parse(args);
                Log.i(tag, command.Description());
                System.Console.WriteLine("Using: " + Config.Settings.ConnectionString);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                command.Execute();
                var totalSeconds = sw.Elapsed.TotalSeconds;
                if (totalSeconds > 5)
                    Log.w(tag, "Wasted " + TimeSpanString.ToString(sw.Elapsed) + " on: " + command.Description());
            }
            catch (Exception caught)
            {
                System.Console.WriteLine(caught.Message);
                Log.e(tag, caught);
                Environment.Exit(1);
            }
            finally
            {
                Log.Flush();
            }
        }

        private static void VerifyConnectionString()
        {
            if (!string.IsNullOrEmpty(Config.Settings.ConnectionString)) return;

            throw new Exception("Set environment variable 'ConnectionString' to connection string or add nin.json config file.");
        }

        const string tag = "CON";
    }
}
