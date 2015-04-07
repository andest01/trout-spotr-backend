using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaWildlifeManagementAreaImporter : ShapefileTableImporter
    {
        public MinnesotaWildlifeManagementAreaImporter(string rootDirectory, IDbConnection connection, string shapefileSrid)
            : base(rootDirectory, connection, "dnr_wma_boundaries_pa", shapefileSrid)
        {
        }

        protected override void PostImportTable()
        {
            TrimGeometry();
            AddSpatialColumn(OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }
    }
}