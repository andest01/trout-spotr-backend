using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TroutDash.DatabaseImporter.Convention.GeometryImporter;

namespace TroutDash.DatabaseImporter.Convention.DatabaseImporter
{
    public class PostGisDatabaseImporter : IDatabaseImporter
    {
        private readonly IDictionary<string, IGeometryImporter> _shapefileImportStrategies;
        private readonly IDatabaseConnection _connection;
        private readonly Lazy<IEnumerable<FileInfo>> _files;
        private readonly DirectoryInfo _directory;
        private readonly DirectoryInfo _sridDirectory;

        public PostGisDatabaseImporter(IDatabaseConnectionFactory factory, DirectoryInfo directory, IDictionary<string, IGeometryImporter> shapefileImportStrategies)
        {
            _directory = directory;
            _shapefileImportStrategies = shapefileImportStrategies;
            _connection = factory.GetConnection(_directory);
            _files = new Lazy<IEnumerable<FileInfo>>(() => GetShapeFiles(directory));
            _sridDirectory = _directory.EnumerateDirectories().SingleOrDefault(d => d.Name == "SridDefinition");
        }

        private string HostName { get { return _connection.HostName; } }
        private string UserName { get { return _connection.UserName; } }

        private string Srid
        {
            get
            {
                if (_sridDirectory == null)
                {
                    return string.Empty;
                }

                var file = new FileInfo(_sridDirectory.EnumerateFiles("*.sql").First().FullName);
                var fileName = Path.GetFileNameWithoutExtension(file.FullName);
                return fileName;
            }
        }

        private const string DropDatabaseTemplate = @"DROP DATABASE IF EXISTS ""{0}"";";

        private const string CreateDatabaseTemplate =
            @"CREATE DATABASE ""{0}"" WITH OWNER = {1} ENCODING = 'UTF8' TABLESPACE = pg_default LC_COLLATE = 'English_United States.1252' LC_CTYPE = 'English_United States.1252' CONNECTION LIMIT = -1";

        private const string AlterNewTableTemplate =
            @"ALTER DATABASE ""{0}"" SET search_path = '$user', public, topology, tiger;";

        private const string AddPostGisExtension = @"CREATE EXTENSION postgis;";
        private const string AddPostGisTopologyExtension = @"CREATE EXTENSION postgis_topology;";
        private const string AddPostGisFuzzyStrMatchExtension = @"CREATE EXTENSION fuzzystrmatch;";
        private const string AddPostGisTigerGeocoderExtension = @"CREATE EXTENSION postgis_tiger_geocoder;";

        public void CreateDatabase()
        {
            DropDatabase(_connection);
            CreateZeDatabase(_connection);
            CreateGisExtensions(_connection);
            AddSpacialReferenceSystem();
        }

        private void AddSpacialReferenceSystem()
        {

            if (_sridDirectory == null)
            {
                return;
            }

            var sridFiles = _sridDirectory.EnumerateFiles("*.sql");
            foreach (var file in sridFiles)
            {
                ExecuteShellCommand.ExecuteSql(file,_connection.DatabaseName, _connection.HostName, _connection.UserName);
            }
        }

        public static void CreateZeDatabase(IDatabaseConnection dbConnection)
        {
            Console.WriteLine("Creating database...");
            var createScript = String.Format(CreateDatabaseTemplate, dbConnection.DatabaseName, dbConnection.UserName);
            var createCommand = String.Format(@"psql -q  --host={0} --username={1} -d postgres --command ""{2}""",
                dbConnection.HostName, dbConnection.UserName, createScript);
            ExecuteShellCommand.ExecuteProcess(createCommand);

            
        }

        private static void CreateGisExtensions(IDatabaseConnection dbConnection)
        {
            Console.WriteLine("Enabling GIS extensions...");
            var alterTableScript = String.Format(AlterNewTableTemplate, dbConnection.DatabaseName);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""",
                dbConnection.HostName,
                dbConnection.UserName, dbConnection.DatabaseName, alterTableScript);
            ExecuteShellCommand.ExecuteProcess(alterCommand);

            var postGisCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""",
                dbConnection.HostName,
                dbConnection.UserName, dbConnection.DatabaseName, AddPostGisExtension);
            ExecuteShellCommand.ExecuteProcess(postGisCommand);

            var topologyCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""",
                dbConnection.HostName,
                dbConnection.UserName, dbConnection.DatabaseName, AddPostGisTopologyExtension);
            ExecuteShellCommand.ExecuteProcess(topologyCommand);

            var fuzzyCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""",
                dbConnection.HostName,
                dbConnection.UserName, dbConnection.DatabaseName, AddPostGisFuzzyStrMatchExtension);
            ExecuteShellCommand.ExecuteProcess(fuzzyCommand);

            var tigerCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""",
                dbConnection.HostName,
                dbConnection.UserName, dbConnection.DatabaseName, AddPostGisTigerGeocoderExtension);
            ExecuteShellCommand.ExecuteProcess(tigerCommand);
        }

        public static void DropDatabase(IDatabaseConnection dbConnection)
        {
            Console.WriteLine("Dropping database...");
            var dropCommand = String.Format(DropDatabaseTemplate, dbConnection.DatabaseName);
            var dropScript = String.Format(@"psql -q  --host={1} --username={2} -d postgres --command ""{0}""",
                dropCommand, dbConnection.HostName, dbConnection.UserName);
            ExecuteShellCommand.ExecuteProcess(dropScript);
        }

        private IEnumerable<FileInfo> GetShapeFiles(DirectoryInfo directory)
        {
            return directory.GetFiles().Where(file => _shapefileImportStrategies.ContainsKey(file.Extension));
        }

        public void Dispose()
        {
            
        }

        public void Import(IDictionary<string, IDatabaseTableProcessor> processors = null)
        {

            foreach (var shapefile in _files.Value)
            {
                var tableName = shapefile.Name;
                using (var processor = GetPostProcessor(processors, tableName))
                {
                    processor.PreProcess();
                    var geometryImporter = _shapefileImportStrategies.Single(i => i.Key == shapefile.Extension).Value;
                    geometryImporter.ImportShape(shapefile, _connection, Srid);
                    processor.Process();
                    processor.PostProcess();
                }
            }
        }

        public string DatabaseName
        {
            get { return _connection.DatabaseName; }
        }

        public IEnumerable<FileInfo> ShapeFiles { get { return _files.Value; } }

        private IDatabaseTableProcessor GetPostProcessor(IDictionary<string, IDatabaseTableProcessor> lookup, string soughtProcessorName)
        {
            lookup = lookup ?? new Dictionary<string, IDatabaseTableProcessor>();
            return lookup.ContainsKey(soughtProcessorName) 
                ? lookup[soughtProcessorName] 
                : new DatabaseTableProcessorBase();
        }
    }
}