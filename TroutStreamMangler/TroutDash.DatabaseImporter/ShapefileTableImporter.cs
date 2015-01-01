using System;
using System.IO;
using System.Linq;

namespace TroutDash.DatabaseImporter
{
    public abstract class ShapefileTableImporter : ITableImporter
    {
        protected readonly string _databaseName;
        protected readonly string _userName;
        protected readonly string _hostName;
        protected readonly string _tableName;
        protected readonly string _shapefileSrid;
        protected readonly DirectoryInfo _rootDirectory;
        protected const string MultilineString = "MultiLineString";
        protected const string OriginalSpatialColumn = "geom";
        protected const string CleanedSpatialColumn = "geom_2d";
        protected const string AlterTableToMultiLineString = @"ALTER TABLE {0} ADD {1} geometry(MultiLineString, {2})";
        protected const string NonUniqueColumnBstarIndex = @"CREATE UNIQUE INDEX ix_{0}_{1} ON public.{0} USING btree (gid ASC NULLS LAST); ALTER TABLE public.{0} CLUSTER ON ix_{0}_{1};";  // table name, column name
        protected const string UpdateMultilineColumn = @"UPDATE {0} SET {1} = ST_Force_2D({2})";

        protected ShapefileTableImporter(string rootDirectory, IDbConnection connection, string tableName, string shapefileSrid)
        {
            _databaseName = connection.DatabaseName;
            _userName = connection.UserName;
            _hostName = connection.HostName;
            _tableName = tableName;
            _shapefileSrid = shapefileSrid;
            _rootDirectory = new DirectoryInfo(rootDirectory);
        }

        public virtual void Dispose()
        {
            
        }

        protected virtual void PreImportTable()
        {
            
        }

        protected virtual void PostImportTable()
        {
            
        }

        protected virtual void AddSpatialColumn(string geometryColumnName, int desiredSrid, string geometryType)
        {
            Console.WriteLine("Adding spatial column for " + _tableName + " with SRID " + desiredSrid);
            const string AddNewSpatialColumn = @"ALTER TABLE {0} ADD geom_{1} geometry({2}, {1})";
            //            const string ConvertColumnToSRID = @"SELECT UpdateGeometrySRID('{0}','geom_{1}', {1})";
            const string CreateSpatialIndex = @"CREATE INDEX {0}_geom_{1}_gist ON public.{0} USING gist(geom_{1})";
            const string UpdateMultilineColumn = @"UPDATE {0} SET geom_{1} = ST_Transform({2}, {3})";

            var alterTableScript = String.Format(AddNewSpatialColumn, _tableName, desiredSrid, geometryType);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, alterTableScript);
            ExecuteShellCommand.ExecuteProcess(alterCommand);

            Console.WriteLine("Reprojecting... ");
            var force = String.Format(UpdateMultilineColumn, _tableName, desiredSrid, geometryColumnName, desiredSrid);
            var forceCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, force);
            ExecuteShellCommand.ExecuteProcess(forceCommand);

            var spatialIndex = String.Format(CreateSpatialIndex, _tableName, desiredSrid);
            var spatialIndexCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""",
                _hostName, _userName, _databaseName, spatialIndex);
            ExecuteShellCommand.ExecuteProcess(spatialIndexCommand);
        }

        protected void ApplyNonUniqueIndexToColumn(string columnName)
        {
            var alterTableScript = String.Format(NonUniqueColumnBstarIndex, _tableName, columnName);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, alterTableScript);
            ExecuteShellCommand.ExecuteProcess(alterCommand);
        }

        public void ImportTable()
        {
            PreImportTable();
            ImportShapefileToTable();
            PostImportTable();
        }

        private void ImportShapefileToTable()
        {
            if (_rootDirectory.Exists == false)
            {
                throw new DirectoryNotFoundException("Directory not found. Cannot import shapefile.");
            }

            // in theory, I've sanitized my input.
            var shapes = Directory.EnumerateFiles(_rootDirectory.FullName, "*.shp", SearchOption.AllDirectories)
                .Select(s => new FileInfo(s));
            foreach (var shapeFile in shapes)
            {
                try
                {
                    var sqlFile = DumpSqlFromShapefile(shapeFile);
                    try
                    {
                        ExecuteShellCommand.ExecuteSql(sqlFile, _databaseName, _hostName, _userName);
                    }

                    finally
                    {
                        sqlFile.Delete();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("------------There was an error trying to import " + shapeFile.Name);
                }
            }
        }

        private FileInfo DumpSqlFromShapefile(FileInfo shapeFile)
        {
            var shortName = Path.GetFileNameWithoutExtension(shapeFile.Name);
            var tableName = _tableName ?? shortName;
            Console.WriteLine("Starting import for file named " + Path.GetFileNameWithoutExtension(shapeFile.Name));
            Console.WriteLine("Creating sql file for " + shapeFile);
            //            var prefix = String.IsNullOrWhiteSpace(shapefileSrid) ? String.Empty : shapefileSrid + ":" + PreferredSrid;
            var prefix = _shapefileSrid;
            const string commandTemplate = "shp2pgsql -d -I -W LATIN1 -s {0} {1} {2} > {3}.sql";
            var command = String.Format(commandTemplate, prefix, shapeFile.FullName, tableName, shortName);
            ExecuteShellCommand.ExecuteProcess(command);

            var sqlFileName = Path.GetFileNameWithoutExtension(shapeFile.FullName) + ".sql";
            return new FileInfo(sqlFileName);
        }

//        protected internal DirectoryInfo MoveTo(string soughtPath)
//        {
//            var soughtDirectory = new DirectoryInfo(_rootDirectory.FullName + @"\" + soughtPath);
//            return soughtDirectory;
//        }
    }
}