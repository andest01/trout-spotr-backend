using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaEasementsImporter : ShapefileTableImporter
    {
        public MinnesotaEasementsImporter(string rootDirectory, IDbConnection connection, string shapefileSrid)
            : base(rootDirectory, connection, "mndnr_fisheries_acquisition", shapefileSrid)
        {
        }

        protected override void PostImportTable()
        {
            AddSpatialColumn(OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }
    }
}