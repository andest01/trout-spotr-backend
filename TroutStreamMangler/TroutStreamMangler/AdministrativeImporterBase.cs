using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using ManyConsole;

namespace TroutStreamMangler
{
    public abstract class AdministrativeImporterBase : ConsoleCommand, IDatabaseImporter
    {
        private const string DropDatabaseTemplate = @"DROP DATABASE IF EXISTS ""{0}"";";

        private const string CreateDatabaseTemplate =
            @"CREATE DATABASE ""{0}"" WITH OWNER = {1} ENCODING = 'UTF8' TABLESPACE = pg_default LC_COLLATE = 'English_United States.1252' LC_CTYPE = 'English_United States.1252' CONNECTION LIMIT = -1";

        private const string AlterNewTableTemplate =
            @"ALTER DATABASE ""{0}"" SET search_path = '$user', public, topology, tiger;";

        private const string AddPostGisExtension = @"CREATE EXTENSION postgis;";
        private const string AddPostGisTopologyExtension = @"CREATE EXTENSION postgis_topology;";
        private const string AddPostGisFuzzyStrMatchExtension = @"CREATE EXTENSION fuzzystrmatch;";
        private const string AddPostGisTigerGeocoderExtension = @"CREATE EXTENSION postgis_tiger_geocoder;";
        protected const string MultilineString = "MultiLineString";
        protected const string NonUniqueColumnBstarIndex = @"CREATE UNIQUE INDEX ix_{0}_{1} ON public.{0} USING btree (gid ASC NULLS LAST); ALTER TABLE public.{0} CLUSTER ON ix_{0}_{1};";  // table name, column name

        protected internal AdministrativeImporterBase()
        {
        }

        protected virtual void PreOnRun()
        {

        }

        protected virtual void PostOnRun()
        {

        }

        public override int Run(string[] remainingArguments)
        {
            return OnRun(remainingArguments);
        }

        protected virtual void AddSpatialColumn(string tableName, string geometryColumnName, int desiredSrid, string geometryType)
        {
            Console.WriteLine("Adding spatial column for " + tableName + " with SRID " + desiredSrid);
            const string AddNewSpatialColumn = @"ALTER TABLE {0} ADD geom_{1} geometry({2}, {1})";
//            const string ConvertColumnToSRID = @"SELECT UpdateGeometrySRID('{0}','geom_{1}', {1})";
            const string CreateSpatialIndex = @"CREATE INDEX {0}_geom_{1}_gist ON public.{0} USING gist(geom_{1})";
            const string UpdateMultilineColumn = @"UPDATE {0} SET geom_{1} = ST_Transform({2}, {3})";

            var alterTableScript = String.Format(AddNewSpatialColumn, tableName, desiredSrid, geometryType);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, alterTableScript);
            ExecuteProcess(alterCommand);

            Console.WriteLine("Reprojecting... ");
            var force = String.Format(UpdateMultilineColumn, tableName, desiredSrid, geometryColumnName, desiredSrid);
            var forceCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, force);
            ExecuteProcess(forceCommand);

            var spatialIndex = String.Format(CreateSpatialIndex, tableName, desiredSrid);
            var spatialIndexCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""",
                HostName, UserName, DatabaseName, spatialIndex);
            ExecuteProcess(spatialIndexCommand);
        }

        protected virtual void InitializeDatabase()
        {
            DropDatabase();
            CreateDatabase();
            AddSpacialReferenceSystem();
        }

        private void CreateDatabase()
        {
            Console.WriteLine("Creating database...");
            var createScript = String.Format(CreateDatabaseTemplate, DatabaseName, UserName);
            var createCommand = String.Format(@"psql -q  --host={0} --username={1} -d postgres --command ""{2}""",
                HostName, UserName, createScript);
            ExecuteProcess(createCommand);

            Console.WriteLine("Enabling GIS extensions...");
            var alterTableScript = String.Format(AlterNewTableTemplate, DatabaseName);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, alterTableScript);
            ExecuteProcess(alterCommand);

            var postGisCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, AddPostGisExtension);
            ExecuteProcess(postGisCommand);

            var topologyCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, AddPostGisTopologyExtension);
            ExecuteProcess(topologyCommand);

            var fuzzyCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, AddPostGisFuzzyStrMatchExtension);
            ExecuteProcess(fuzzyCommand);

            var tigerCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, AddPostGisTigerGeocoderExtension);
            ExecuteProcess(tigerCommand);
        }

        private void DropDatabase()
        {
            Console.WriteLine("Dropping database...");
            var dropCommand = String.Format(DropDatabaseTemplate, DatabaseName);
            var dropScript = String.Format(@"psql -q  --host={1} --username={2} -d postgres --command ""{0}""",
                dropCommand, HostName, UserName);
            ExecuteProcess(dropScript);
        }

        protected internal abstract string Srid { get; }

        protected internal void ExecuteProcess(string command)
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
            process.WaitForExit(80000);
        }

        protected internal abstract int OnRun(string[] remainingArguments);

        protected internal virtual FileInfo OnAddSpacialReferenceSystem()
        {
            return null;
        }

        private void AddSpacialReferenceSystem()
        {
            Console.WriteLine("Adding custom spatial reference systems...");
            var file = OnAddSpacialReferenceSystem();
            if (file == null)
            {
                return;
            }

            ExecuteSql(file);
        }

        protected void ApplyNonUniqueIndexToColumn(string tableName, string columnName)
        {
            var alterTableScript = String.Format(NonUniqueColumnBstarIndex, tableName, columnName);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, alterTableScript);
            ExecuteProcess(alterCommand);
        }

        private void ExecuteSql(FileInfo sql)
        {
            Console.WriteLine("Starting to execute sql named " + sql.Name);
            const string commandTemplate = @"psql -q -d {0} -f {1} --host={2} --username={3}";
            string command = String.Format(commandTemplate, DatabaseName, sql.FullName, HostName, UserName);
            ExecuteProcess(command);
        }

        protected internal void Import(DirectoryInfo targetDirectory)
        {
            var importer = new ImportShapefile
            {
                DatabaseName = DatabaseName,
                HostName = HostName,
                UserName = UserName,
                RootDirectory = targetDirectory,
                shapefileSrid = Srid //"926915"
            };

            importer.Run(new string[0]);
        }

        protected internal DirectoryInfo MoveTo(string soughtPath)
        {
            var soughtDirectory = new DirectoryInfo(RootDirectory.FullName + @"\" + soughtPath);
            return soughtDirectory;
        }

        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public DirectoryInfo RootDirectory { get; set; }
    }
}