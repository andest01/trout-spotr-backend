using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaRegulationSectionsImporter : ShapefileTableImporter
    {
        public MinnesotaRegulationSectionsImporter(string rootDirectory, IDbConnection connection, string shapefileSrid)
            : base(rootDirectory, connection, "strm_regsln3", shapefileSrid)
        {
        }

        protected override void PostImportTable()
        {
            AddSpatialColumn(OriginalSpatialColumn, 4326, "Multipolygon");
            AddSpatialColumn(OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }
    }
}