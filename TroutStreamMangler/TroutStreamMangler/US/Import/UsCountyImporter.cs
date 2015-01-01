using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.US.Import
{
    public class UsCountyImporter : ShapefileTableImporter
    {
        public UsCountyImporter(string rootDirectory, IDbConnection connection, string tableName, string shapefileSrid)
            : base(rootDirectory, connection, "dnr_hydro_features_all", shapefileSrid)
        {
        }

        protected override void PostImportTable()
        {
            AddSpatialColumn(OriginalSpatialColumn, 4326, "Multipolygon");
            AddSpatialColumn(OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }
    }
}