using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.US.Import
{
    public class UsStateImporter : ShapefileTableImporter
    {
        public UsStateImporter(string rootDirectory, IDbConnection connection, string tableName, string shapefileSrid)
            : base(rootDirectory, connection, "states", shapefileSrid)
        {
        }

        protected override void PostImportTable()
        {
            AddSpatialColumn(OriginalSpatialColumn, 4326, "Multipolygon");
            AddSpatialColumn(OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }
    }
}