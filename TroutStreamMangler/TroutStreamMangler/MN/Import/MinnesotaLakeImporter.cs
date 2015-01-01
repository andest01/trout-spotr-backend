using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaLakeImporter : ShapefileTableImporter
    {
        public MinnesotaLakeImporter(string rootDirectory, IDbConnection connection, string shapefileSrid)
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