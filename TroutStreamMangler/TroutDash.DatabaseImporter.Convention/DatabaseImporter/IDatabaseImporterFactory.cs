using System.IO;

namespace TroutDash.DatabaseImporter.Convention.DatabaseImporter
{
    public interface IDatabaseImporterFactory
    {
        /// <summary>
        /// Creates a new database with the directory name and returns the importer
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        IDatabaseImporter CreateDatabaseRepository(DirectoryInfo directory);
    }
}