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
                return new DatabaseConnection(c.Database, "postgres", c.Host);
            }
        }

        private static IDatabaseConnection GetTroutDashConnection()
        {
            return GetDbConnection(ConfigurationManager.ConnectionStrings["TroutDash"].ConnectionString);
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
            var builder = new ContainerBuilder();

            builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();
            builder.RegisterType<DatabaseImporterFactory>().As<IDatabaseImporterFactory>();
            var shapeImporterFactory = new Dictionary<string, IGeometryImporter>();
//            var importer = msdaf.CreateDatabaseImporter<PostGisDatabaseImporter>(myDirectory);

            shapeImporterFactory.Add(".shp", new ShapefileImporter());

            builder.Register((i) => shapeImporterFactory).As<IDictionary<string, IGeometryImporter>>();
            builder.RegisterType<DatabaseManifest>().As<IDatabaseManifest>();

            var container = builder.Build();

            var manifest = container.Resolve<IDatabaseManifest>();
            var importers = manifest.GetDatabaseImporters(new DirectoryInfo("."));
            
            var usImporter = importers.Single(i => i.DatabaseName == "us_import");
            usImporter.CreateDatabase();
            usImporter.Import();
//

            var states = importers.Where(i => i.DatabaseName != "us_import");
            foreach (var importer in states)
                using (importer)
                {
                    importer.CreateDatabase();
                    importer.Import();
//                    importer.ShapeFiles
                }


            PostImportUsData(null);
            PostImportMinnesotaData(null);

            ExportUsData(null);
            ExportMinnesotaData(null);

             return 0;
        }

        private static void PostImportMinnesota(TroutDash.DatabaseImporter.Convention.DatabaseImporter.IDatabaseImporter importer)
        {
//            importer.ShapeFiles.First().
        }

        protected const string NonUniqueColumnBstarIndex = @"CREATE UNIQUE INDEX ix_{0}_{1} ON public.{0} USING btree (gid ASC NULLS LAST); ALTER TABLE public.{0} CLUSTER ON ix_{0}_{1};";  // table name, column name


//        protected void ApplyNonUniqueIndexToColumn(string tableName, string columnName, TroutDash.DatabaseImporter.Convention.DatabaseImporter.IDatabaseImporter importer)
//        {
//            var alterTableScript = String.Format(NonUniqueColumnBstarIndex, tableName, columnName);
//            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", importer.HostName,
//                UserName, DatabaseName, alterTableScript);
//            ExecuteProcess(alterCommand);
//        }

//        private static void ConfigureMinnesota(ContainerBuilder builder)
//        {
//            var mnDbConnection = GetMnConnection();
//            var mnRootDirectory = new DirectoryInfo(
//                @"C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\Data");
//            ConfigureDatabaseImporter<MinnesotaDatabaseImport>(builder, mnRootDirectory, mnDbConnection);
//        }
//
//        private static void ConfigureUs(ContainerBuilder builder)
//        {
//            var usDbConnection = GetUsConnection();
//            var usRootDirectory = new DirectoryInfo(
//                @"C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\US\Data");
//            ConfigureDatabaseImporter<UsDatabaseImport>(builder, usRootDirectory, usDbConnection);
//        }

//        private static void ConfigureDatabaseImporter<T>(ContainerBuilder builder, DirectoryInfo directory, IDatabaseConnection dbConnection)
//        {
//            builder.RegisterType<T>()
//                .AsSelf()
//                .WithParameter(new ResolvedParameter((a, b) => a.ParameterType == typeof(IDbConnection),
//                    (a, b) => dbConnection))
//                .WithParameter(new ResolvedParameter((a, b) => a.ParameterType == typeof(ITableImporterManifest),
//                    (a, b) => new MinnesotaTableManifest(directory, dbConnection)));
//        }
//
        private static void ExportMinnesotaData(string[] args)
        {
            var minnesotaExporter = new ExportMinnesotaData()
            {
                FileLocation = @"MN/Data/Restrictions/regulations.json"
            };
            minnesotaExporter.Run(args);
        }

        private static void ExportUsData(string[] args)
        {
            var usExporter = new ExportUsData()
            {
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