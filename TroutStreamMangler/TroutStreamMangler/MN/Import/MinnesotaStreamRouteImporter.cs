using System;
using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaStreamRouteImporter : ShapefileTableImporter
    {
        public MinnesotaStreamRouteImporter(string rootDirectory, IDbConnection connection, string tableName, string shapefileSrid)
            : base(rootDirectory, connection, tableName, shapefileSrid)
        {
        }

        protected override void PostImportTable()
        {
            var alterScript = String.Format(AlterTableToMultiLineString, _tableName, CleanedSpatialColumn, _shapefileSrid);

            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, alterScript);
            ExecuteShellCommand.ExecuteProcess(alterCommand);

            var updateTableScript = String.Format(UpdateMultilineColumn, _tableName, CleanedSpatialColumn, OriginalSpatialColumn);
            var updateCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, updateTableScript);
            ExecuteShellCommand.ExecuteProcess(updateCommand);
            ApplyNonUniqueIndexToColumn("kittle_nbr");

            AddSpatialColumn(CleanedSpatialColumn, 4326, MultilineString);
//            AddSpatialColumn(CleanedSpatialColumn, ImportShapefile.PreferredSrid, MultilineString);

            
        }
    }
}