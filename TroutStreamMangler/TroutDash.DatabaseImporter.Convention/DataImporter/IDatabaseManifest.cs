using System.Collections.Generic;
using System.IO;
using System.Linq;
using TroutDash.DatabaseImporter.Convention.DatabaseImporter;

namespace TroutDash.DatabaseImporter.Convention.DataImporter
{
    public interface IDatabaseManifest
    {
        IEnumerable<IDatabaseImporter> GetDatabaseImporters(DirectoryInfo rootDirectory);
    }

    public class DatabaseManifest : IDatabaseManifest
    {
        private readonly IDatabaseConnectionFactory _factory;
        private readonly IDatabaseImporterFactory _importerFactory;

        public DatabaseManifest(IDatabaseConnectionFactory factory, IDatabaseImporterFactory importerFactory)
        {
            _factory = factory;
            _importerFactory = importerFactory;
        }

        public IEnumerable<IDatabaseImporter> GetDatabaseImporters(DirectoryInfo rootDirectory)
        {
            // find all folders that have shapefiles.
            var directoriesWithShapes = rootDirectory.EnumerateDirectories()
                .Where(i => i.GetFiles("*.shp").Any());

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var directory in directoriesWithShapes)
            {
                var dbConnection = _factory.GetConnection(directory);
                var importer = _importerFactory.CreateDatabaseRepository(directory);
                yield return importer;
            }
        }
    }
}