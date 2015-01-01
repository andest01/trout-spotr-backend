using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ManyConsole;
using NetTopologySuite.Index.Bintree;

namespace TroutStreamMangler
{
    /// <summary>
    /// This is a wrapper for shp2pgsql --> *.sql --> psql dump
    /// It imports a given shapefile to a given tablename.
    /// </summary>
    public class ImportShapefile : ConsoleCommand, IDatabaseImporter
    {
        private int _timeout = 5*60*1000;
        public static readonly int PreferredSrid = 3857;

        public ImportShapefile()
        {
            IsCommand("ImportShapefile", "Import a given shapefile to a given database");
            HasOption("timeout=", "Set seconds before automatic timeout and exit", v => _timeout = Convert.ToInt32(v));
            HasOption("tableName=", "Set the resultant table name (default is shapefile's name)", v => TableName = v);
            HasRequiredOption("databaseName=", "Required database name", s => { DatabaseName = s; });
            HasRequiredOption("hostName=", "Required host name (e.g. localhost)", s => { HostName = s; });
            HasRequiredOption("username=", "Required user name (e.g. postgres or admin)", s => { UserName = s; });
            HasOption("srid=", "incomming shapefile's srid", s => shapefileSrid = s);
            HasRequiredOption("shapeFileDirectory=", "Required location of shape files",
                s => { RootDirectory = new DirectoryInfo(s); });
        }

        public DirectoryInfo RootDirectory { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public string TableName { get; set; }
        public string shapefileSrid { get; set; }

        public override int Run(string[] remainingArguments)
        {
            if (RootDirectory.Exists == false)
            {
                throw new DirectoryNotFoundException("Directory not found. Cannot import shapefile.");
            }

            // in theory, I've sanitized my input.
            var shapes = Directory.EnumerateFiles(RootDirectory.FullName, "*.shp", SearchOption.AllDirectories)
                .Select(s => new FileInfo(s));
            foreach (var shapeFile in  shapes)
            {
                try
                {
                    var sqlFile = DumpSqlFromShapefile(shapeFile);
                    try
                    {
                        ExecuteSql(sqlFile);
                    }

                    finally
                    {
                        sqlFile.Delete();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("------------There was an error trying to import " + shapeFile.Name);
                }
            }
            return 0;
        }

        private void ExecuteSql(FileInfo sql)
        {
            Console.WriteLine("Starting to execute sql named " + sql.Name);
            const string commandTemplate = @"psql -q -d {0} -f {1} --host={2} --username={3}";
            string command = String.Format(commandTemplate, DatabaseName, sql.FullName, HostName, UserName);
            ExecuteProcess(command);
        }

        private FileInfo DumpSqlFromShapefile(FileInfo shapeFile)
        {
            var shortName = Path.GetFileNameWithoutExtension(shapeFile.Name);
            var tableName = TableName ?? shortName;
            Console.WriteLine("Starting import for file named " + Path.GetFileNameWithoutExtension(shapeFile.Name));
            Console.WriteLine("Creating sql file for " + shapeFile);
//            var prefix = String.IsNullOrWhiteSpace(shapefileSrid) ? String.Empty : shapefileSrid + ":" + PreferredSrid;
            var prefix = shapefileSrid;
            const string commandTemplate = "shp2pgsql -d -I -W LATIN1 -s {0} {1} {2} > {3}.sql";
            var command = String.Format(commandTemplate, prefix, shapeFile.FullName, tableName, shortName);
            ExecuteProcess(command);

            var sqlFileName = Path.GetFileNameWithoutExtension(shapeFile.FullName) + ".sql";
            return new FileInfo(sqlFileName);
        }

        private void ExecuteProcess(string command)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = false,
                RedirectStandardOutput = false,
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(_timeout);
        }
    }
}