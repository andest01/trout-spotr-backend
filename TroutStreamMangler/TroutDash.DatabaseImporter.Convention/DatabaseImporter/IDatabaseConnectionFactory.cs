using System.IO;

namespace TroutDash.DatabaseImporter.Convention.DatabaseImporter
{
    public interface IDatabaseConnectionFactory
    {
        IDatabaseConnection GetConnection(DirectoryInfo directory);
    }

    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        public IDatabaseConnection GetConnection(DirectoryInfo directory)
        {
            var databaseName = directory.Name;
            return new DatabaseConnection(databaseName, "localhost", "postgres");
        }
    }
}