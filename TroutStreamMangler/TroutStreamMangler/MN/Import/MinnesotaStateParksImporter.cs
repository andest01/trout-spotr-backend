using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaStateParksImporter : ShapefileTableImporter
    {
        public MinnesotaStateParksImporter(string rootDirectory, IDbConnection connection, string shapefileSrid)
            : base(rootDirectory, connection, "dnr_stat_plan_areas_prk", shapefileSrid)
        {
        }

        protected override void PostImportTable()
        {
            AddSpatialColumn(OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }
    }
}