using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaWildlifeManagementAreaImporter : ShapefileTableImporter
    {
        public MinnesotaWildlifeManagementAreaImporter(string rootDirectory, IDbConnection connection, string shapefileSrid)
            : base(rootDirectory, connection, "dnr_wildlife_management_area_boundaries_publicly_accessible", shapefileSrid)
        {
        }

        protected override void PostImportTable()
        {
            AddSpatialColumn(OriginalSpatialColumn, 4326, "Multipolygon");
            AddSpatialColumn(OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }
    }
}