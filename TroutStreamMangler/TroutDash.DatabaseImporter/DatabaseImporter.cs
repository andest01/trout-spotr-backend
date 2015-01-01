using System;
using System.Collections.Generic;
using System.IO;

namespace TroutDash.DatabaseImporter
{
    public abstract class DatabaseImporter : IDatabaseImporter
    {
        private const string DropDatabaseTemplate = @"DROP DATABASE IF EXISTS ""{0}"";";
        private const string AddPostGisExtension = @"CREATE EXTENSION postgis;";
        private const string AddPostGisTopologyExtension = @"CREATE EXTENSION postgis_topology;";
        private const string AddPostGisFuzzyStrMatchExtension = @"CREATE EXTENSION fuzzystrmatch;";
        private const string AddPostGisTigerGeocoderExtension = @"CREATE EXTENSION postgis_tiger_geocoder;";
        private const string CreateDatabaseTemplate =
            @"CREATE DATABASE ""{0}"" WITH OWNER = {1} ENCODING = 'UTF8' TABLESPACE = pg_default LC_COLLATE = 'English_United States.1252' LC_CTYPE = 'English_United States.1252' CONNECTION LIMIT = -1";
        private const string AlterNewTableTemplate =
            @"ALTER DATABASE ""{0}"" SET search_path = '$user', public, topology, tiger;";

        private readonly string _databaseName;
        private readonly string _hostName;
        private readonly string _userName;
        protected readonly ITableImporterManifest _tables;

        protected DatabaseImporter(IDbConnection connection, ITableImporterManifest tableManifest)
        {
            _databaseName = connection.DatabaseName;
            _hostName = connection.HostName;
            _userName = connection.UserName;
            _tables = tableManifest;
        }

        public void InitializeDatabase()
        {
            DropDatabase();
            CreateDatabase();
            AddSpatialReferenceSystem();
        }

        private void DropDatabase()
        {
            Console.WriteLine("Dropping database...");
            var dropCommand = String.Format(DropDatabaseTemplate, _databaseName);
            var dropScript = String.Format(@"psql -q  --host={1} --username={2} -d postgres --command ""{0}""",
                dropCommand, _hostName, _userName);
            ExecuteShellCommand.ExecuteProcess(dropScript);
        }

        protected virtual IEnumerable<FileInfo> SpatialReferenceSystemScripts()
        {
            return new FileInfo[0];
        }

        private void CreateDatabase()
        {
            Console.WriteLine("Creating database...");
            var createScript = String.Format(CreateDatabaseTemplate, _databaseName, _userName);
            var createCommand = String.Format(@"psql -q  --host={0} --username={1} -d postgres --command ""{2}""",
                _hostName, _userName, createScript);
            ExecuteShellCommand.ExecuteProcess(createCommand);

            Console.WriteLine("Enabling GIS extensions...");
            var alterTableScript = String.Format(AlterNewTableTemplate, _databaseName);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, alterTableScript);
            ExecuteShellCommand.ExecuteProcess(alterCommand);

            var postGisCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, AddPostGisExtension);
            ExecuteShellCommand.ExecuteProcess(postGisCommand);

            var topologyCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, AddPostGisTopologyExtension);
            ExecuteShellCommand.ExecuteProcess(topologyCommand);

            var fuzzyCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, AddPostGisFuzzyStrMatchExtension);
            ExecuteShellCommand.ExecuteProcess(fuzzyCommand);

            var tigerCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, AddPostGisTigerGeocoderExtension);
            ExecuteShellCommand.ExecuteProcess(tigerCommand);
        }

        private void AddSpatialReferenceSystem()
        {
            Console.WriteLine("Adding custom spatial reference systems...");
            var files = SpatialReferenceSystemScripts();
            foreach (var file in files)
            {
                if (file == null)
                {
                    return;
                }

                ExecuteShellCommand.ExecuteSql(file, _databaseName, _hostName, _userName);
            }
        }

        protected virtual void PreImportTables()
        {
            
        }

        protected virtual void PostImportTables()
        {

        }

        public void ImportTables()
        {
            foreach (var tableImporter in _tables.TableImporters)
            {
                try
                {
                    PreImportTables();
                    tableImporter.ImportTable();
                    PostImportTables();
                }
                catch (Exception)
                {

                }
                finally
                {
                    tableImporter.Dispose();
                }
            }
        }

        public void Dispose()
        {
            // perform any cleanup here.
        }
    }
}