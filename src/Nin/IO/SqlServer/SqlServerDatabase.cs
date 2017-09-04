using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using Nin.Configuration;
using Nin.Diagnostic;

namespace Nin.IO.SqlServer
{
    public static class SqlServerDatabase
    {
        public static void WipeAndCreate(string scriptDirectory)
        {
            var initialCatalog = new SqlConnectionStringBuilder(Config.Settings.ConnectionString).InitialCatalog;
            Drop(initialCatalog);

            CreateEmpty(initialCatalog);
            UpgradeExisting(scriptDirectory);
        }

        public static void CreateEmpty(string catalog)
        {
            var datasource = new SqlConnectionStringBuilder(Config.Settings.ConnectionString).DataSource;
            Log.i("DB", $"Creating database \'{catalog}\' in \'{datasource}\'.");
            MasterDatabase.Execute($"CREATE DATABASE {catalog}");
        }

        public static void UpgradeExisting(string schemaScriptDirectory)
        {
            var files = Directory.GetFiles(schemaScriptDirectory, "*.sql");
            Array.Sort(files);
            foreach (var scriptFile in files)
                RunUpgradeScript(new SqlScript(scriptFile));
        }

        static void RunUpgradeScript(SqlScript scriptFile)
        {
            try
            {
                scriptFile.Execute();
            }
            catch (Exception caught)
            {
                throw new Exception($"Script '{scriptFile}' failed: {caught.Message}", caught);
            }
        }

        static void Drop(string catalog)
        {
            Log.i("DB", "Dropping database '" + catalog + "'.");
            MasterDatabase.Execute($@"WHILE EXISTS(select NULL from sys.databases where name='{catalog}')
                BEGIN
                    DECLARE @SQL varchar(max)
                    SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
                    FROM MASTER..SysProcesses WHERE DBId = DB_ID(N'{catalog}') AND SPId <> @@SPId
                    EXEC(@SQL)
                    DROP DATABASE [{catalog}]
                END");
        }
    }

    public class SqlScript
    {
        public string Name { get; }
        readonly string path;

        public SqlScript(string scriptFilePath)
        {
            path = scriptFilePath;
            Name = Path.GetFileNameWithoutExtension(scriptFilePath);
        }

        public void Execute()
        {
            if (RevisionLog.ContainsRevision(this))
            {
                Log.v("DB", $"Skipping upgrade script \'{Name}\' as it is already in database.");
                return;
            }

            Log.i("DB", $"Upgrading with script \'{Name}\'");
            using (var transaction = new TransactionScope()) 
            {
                foreach (string batch in GetBatches())
                    SqlStatement.Execute(batch);
                RevisionLog.Add(this);
                transaction.Complete();
            }
        }

        IEnumerable<string> GetBatches()
        {
            var sql = File.ReadAllText(path);
            // Split by "GO" statements
            var statements = Regex.Split(sql, @"^\s*GO\s* ($ | \-\- .*$)",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

            return statements.Where(x => !string.IsNullOrWhiteSpace(x));
        }
    }

    public static class RevisionLog
    {
        public static void Add(SqlScript script)
        {
            using (var q = new SqlStatement("INSERT INTO RevisionLog (name) VALUES (@name)"))
            {
                q.AddParameter("@name", script.Name);
                q.ExecuteNonQuery();
            }
        }

        public static bool ContainsRevision(SqlScript script)
        {
            CreateRevisionLogTable();

            using (var q = new SqlStatement("SELECT COUNT(*) FROM RevisionLog WHERE name=@name"))
            {
                q.AddParameter("@name", script.Name);
                return (int)q.ExecuteScalar() > 0;
            }
        }

        static void CreateRevisionLogTable()
        {
            SqlStatement.Execute(@"
              IF NOT EXISTS(SELECT * FROM sys.objects 
                WHERE object_id = OBJECT_ID(N'[dbo].[RevisionLog]') AND type in (N'U')) BEGIN
                CREATE TABLE RevisionLog ( 
	              id      int identity(1,1)  NOT NULL,
	              name    varchar(255) NOT NULL,
	              created datetime NOT NULL DEFAULT (GETDATE()));
                ALTER TABLE RevisionLog ADD CONSTRAINT UQ_RevisionLog_name UNIQUE (name);
                ALTER TABLE RevisionLog ADD CONSTRAINT PK_RevisionLog PRIMARY KEY CLUSTERED (id);
             END");
        }
    }

    static class MasterDatabase
    {
        public static void Execute(string cmdText)
        {
            using (var q = new SqlStatement(cmdText, GetMasterConnectionString()))
                q.ExecuteNonQuery();
        }

        static string GetMasterConnectionString()
        {
            var cs = new SqlConnectionStringBuilder(Config.Settings.ConnectionString) {InitialCatalog = ""};
            return cs.ConnectionString;
        }
    }
}