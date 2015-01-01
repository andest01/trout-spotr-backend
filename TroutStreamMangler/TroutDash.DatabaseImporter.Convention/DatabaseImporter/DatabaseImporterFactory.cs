using System;
using System.Collections.Generic;
using System.IO;
using TroutDash.DatabaseImporter.Convention.GeometryImporter;

namespace TroutDash.DatabaseImporter.Convention.DatabaseImporter
{
    public class DatabaseImporterFactory : IDatabaseImporterFactory
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IDictionary<string, IGeometryImporter> _acceptableFiles;

        public DatabaseImporterFactory(IDatabaseConnectionFactory connectionFactory, IDictionary<string, IGeometryImporter> acceptableFiles)
        {
            _connectionFactory = connectionFactory;
            _acceptableFiles = acceptableFiles;
        }

        public IDatabaseImporter CreateDatabaseRepository(DirectoryInfo directory)
        {
            return new PostGisDatabaseImporter(_connectionFactory, directory, _acceptableFiles);
        }
    }
}