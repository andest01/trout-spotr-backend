using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Autofac;
using ManyConsole;
using Npgsql;
using TroutDash.DatabaseImporter.Convention;
using TroutDash.DatabaseImporter.Convention.DatabaseImporter;
using TroutDash.DatabaseImporter.Convention.DataImporter;
using TroutDash.DatabaseImporter.Convention.GeometryImporter;
using TroutStreamMangler.MN;
using TroutStreamMangler.US;

namespace TroutStreamMangler
{
//    public static class Kernel
//    {
//        public static object Container()
//        {
//            var builder = new ContainerBuilder();
//        }
//    }

    internal class Program
    {
        private static IDatabaseConnection GetDbConnection(string connection)
        {
            using (var c = new NpgsqlConnection(connection))
            {
                return new DatabaseConnection(c.Database, c.Host, "postgres");
            }
        }

        private static IDatabaseConnection GetTroutDashConnection()
        {
            return GetDbConnection(ConfigurationManager.ConnectionStrings["troutdash2"].ConnectionString);
        }

        private static IDatabaseConnection GetMnConnection()
        {
            return GetDbConnection(ConfigurationManager.ConnectionStrings["mn_import"].ConnectionString);
        }

        private static IDatabaseConnection GetUsConnection()
        {
            return GetDbConnection(ConfigurationManager.ConnectionStrings["us_import"].ConnectionString);
        }

        private static int Main(string[] args)
        {
//            var builder = new ContainerBuilder();
//
//            builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();
//            builder.RegisterType<DatabaseImporterFactory>().As<IDatabaseImporterFactory>();
//            var shapeImporterFactory = new Dictionary<string, IGeometryImporter>();
////            var importer = msdaf.CreateDatabaseImporter<PostGisDatabaseImporter>(myDirectory);
//
//            shapeImporterFactory.Add(".shp", new ShapefileImporter());
//
//            builder.Register((i) => shapeImporterFactory).As<IDictionary<string, IGeometryImporter>>();
//            builder.RegisterType<DatabaseManifest>().As<IDatabaseManifest>();
//
//            var container = builder.Build();
//
//            var manifest = container.Resolve<IDatabaseManifest>();
//            var importers = manifest.GetDatabaseImporters(new DirectoryInfo("."));
////            
//            var usImporter = importers.Single(i => i.DatabaseName == "us_import");
//            usImporter.CreateDatabase();
//            usImporter.Import();
//
//
//            var states = importers.Where(i => i.DatabaseName != "us_import");
//            foreach (var importer in states)
//                using (importer)
//                {
//                    importer.CreateDatabase();
//                    importer.Import();
//                }
//
//
//
//            PostImportUsData(null);
//            PostImportMinnesotaData(null);

            var dbConnection = GetTroutDashConnection();
            var mnConnection = GetMnConnection();
            // dropdb -h localhost -U postgres --if-exists TroutDash2
//            PostGisDatabaseImporter.DropDatabase(dbConnection);
//            PostGisDatabaseImporter.CreateZeDatabase(dbConnection);
//
//            var restoreDbCommand = String.Format(@"psql -U {0} -d {1} -a -f {2}", dbConnection.UserName, dbConnection.DatabaseName, "Streams_backup_2015_06_24_1_schema.backup");
//            ExecuteShellCommand.ExecuteProcess(restoreDbCommand);
//
//            ExportUsData(null, dbConnection);
            ExportMinnesotaData(null, dbConnection, mnConnection);

             return 0;
        }

        private static void PostImportMinnesota(TroutDash.DatabaseImporter.Convention.DatabaseImporter.IDatabaseImporter importer)
        {
//            importer.ShapeFiles.First().
        }

        protected const string NonUniqueColumnBstarIndex = @"CREATE UNIQUE INDEX ix_{0}_{1} ON public.{0} USING btree (gid ASC NULLS LAST); ALTER TABLE public.{0} CLUSTER ON ix_{0}_{1};";  // table name, column name

        private static void ExportMinnesotaData(string[] args, IDatabaseConnection dbConnection, IDatabaseConnection dbMnImportConnection)
        {
            var minnesotaExporter = new ExportMinnesotaData(dbConnection, dbMnImportConnection, new CentroidResetter(dbConnection))
            {
                FileLocation = @"MN/Data/Restrictions/regulations.json",
                StreamMetadataFileLocation = @"MN/Data/Streams/StreamMetadata.csv",
                
            };
            minnesotaExporter.Run(args);
        }

        private static void ExportUsData(string[] args, IDatabaseConnection dbConnection)
        {
            var usConnection = GetUsConnection();
            var usExporter = new ExportUsData(dbConnection, usConnection)
            {
                RegionCsv = @"US/Data/Regions/"
            };
            usExporter.Run(args);
        }

        private static void PostImportMinnesotaData(string[] args)
        {
            var t = new ImportMinnesotaData()
            {
                RootDirectory =
                    new DirectoryInfo(
                        @"C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\Data"),
                DatabaseName = "mn_import",
                HostName = "localhost",
                UserName = "postgres",
            };

            t.Run(args);
        }

        private static void PostImportUsData(string[] args)
        {
            var us = new ImportUsData()
            {
                RootDirectory =
                    new DirectoryInfo(
                        @"C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\US\Data"),
                DatabaseName = "us_import",
                HostName = "localhost",
                UserName = "postgres"
            };

            us.Run(args);
        }

        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof (Program));
        }
    }

    public class InitializeDatabase : ConsoleCommand
    {
        public InitializeDatabase()
        {
            IsCommand("Initialize", "Initialize the database. Load county and state data.");
            HasRequiredOption("shapeFiles=", "Required location of shape files", s => { ShapefileLocation = s; });
        }

        protected string ShapefileLocation { get; set; }

        public override int Run(string[] remainingArguments)
        {
            Console.WriteLine("TODO: this.");
            return 0;
        }
    }
}