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
        public static readonly int PreferredSrid = 4326;

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
            
            return 0;
        }
    }
}