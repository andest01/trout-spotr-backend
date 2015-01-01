using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.US.Import
{
    public class UsDatabaseImport : DatabaseImporter
    {
        public UsDatabaseImport(IDbConnection connection, ITableImporterManifest tableManifest) : base(connection, tableManifest)
        {
        }
    }
}