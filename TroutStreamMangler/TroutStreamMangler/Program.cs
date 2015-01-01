using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Autofac;
using Autofac.Core;
using Devart.Common;
using ManyConsole;
using Npgsql;
using TroutDash.DatabaseImporter;
using TroutDash.DatabaseImporter.Convention.DatabaseImporter;
using TroutStreamMangler.MN;
using TroutStreamMangler.MN.Import;
using TroutStreamMangler.US;
using TroutStreamMangler.US.Import;

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
        private static IDbConnection GetDbConnection(string connection)
        {
            using (var c = new NpgsqlConnection(connection))
            {
                return new DbConnection(c.Database, "postgres", c.Host);
            }
        }

        private static IDbConnection GetTroutDashConnection()
        {
            return GetDbConnection(ConfigurationManager.ConnectionStrings["TroutDash"].ConnectionString);
        }

        private static IDbConnection GetMnConnection()
        {
            return GetDbConnection(ConfigurationManager.ConnectionStrings["mn_import"].ConnectionString);
        }

        private static IDbConnection GetUsConnection()
        {
            return GetDbConnection(ConfigurationManager.ConnectionStrings["us_import"].ConnectionString);
        }

        private static int Main(string[] args)
        {
            var builder = new ContainerBuilder();

            // Minnesota.
//            ConfigureMinnesota(builder);
//            ConfigureUs(builder);
            builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();
            builder.RegisterType<DatabaseImporterFactory>().As<IDatabaseImporterFactory>();
            var container = builder.Build();
//            var result = container.Resolve<MinnesotaDatabaseImport>();
//            var usResult = container.Resolve<UsDatabaseImport>();

//            var mn = new MinnesotaDatabaseImport()
//            // locate any commands in the assembly (or use an IoC container, or whatever source)
//            var commands = GetCommands();
//            ImportUsData(args);
//            ExportUsData(args);
//            ImportMinnesotaData(args);
//            ExportMinnesotaData(args);
            return 0;
        }

        private static void ConfigureMinnesota(ContainerBuilder builder)
        {
            var mnDbConnection = GetMnConnection();
            var mnRootDirectory = new DirectoryInfo(
                @"C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\Data");
            ConfigureDatabaseImporter<MinnesotaDatabaseImport>(builder, mnRootDirectory, mnDbConnection);
        }

        private static void ConfigureUs(ContainerBuilder builder)
        {
            var usDbConnection = GetUsConnection();
            var usRootDirectory = new DirectoryInfo(
                @"C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\US\Data");
            ConfigureDatabaseImporter<UsDatabaseImport>(builder, usRootDirectory, usDbConnection);
        }

        private static void ConfigureDatabaseImporter<T>(ContainerBuilder builder, DirectoryInfo directory, IDbConnection dbConnection)
        {
            builder.RegisterType<T>()
                .AsSelf()
                .WithParameter(new ResolvedParameter((a, b) => a.ParameterType == typeof(IDbConnection),
                    (a, b) => dbConnection))
                .WithParameter(new ResolvedParameter((a, b) => a.ParameterType == typeof(ITableImporterManifest),
                    (a, b) => new MinnesotaTableManifest(directory, dbConnection)));
        }

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

        private static void ImportMinnesotaData(string[] args)
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

        private static void ImportUsData(string[] args)
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