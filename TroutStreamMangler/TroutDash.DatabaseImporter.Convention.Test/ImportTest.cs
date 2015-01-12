using System;
using System.Collections.Generic;
using System.IO;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TroutDash.DatabaseImporter.Convention.DatabaseImporter;
using TroutDash.DatabaseImporter.Convention.DataImporter;
using TroutDash.DatabaseImporter.Convention.GeometryImporter;

namespace TroutDash.DatabaseImporter.Convention.Test
{
    [TestClass]
    public class ImportTest
    {
        [TestMethod]
        public void Import()
        {
            var packager = A.Fake<IDataPackager>();
            A.CallTo(() => packager.Unpack())
                .Returns(new DirectoryInfo("."));

            var connectionFactory = new DatabaseConnectionFactory();
            var shapeImporterFactory = new Dictionary<string, IGeometryImporter>();
//            var importer = msdaf.CreateDatabaseImporter<PostGisDatabaseImporter>(myDirectory);

            shapeImporterFactory.Add(".shp", new ShapefileImporter());
            var manifest = new DatabaseManifest(connectionFactory, new DatabaseImporterFactory(connectionFactory, shapeImporterFactory));
            var items = manifest.GetDatabaseImporters(packager.Unpack());
            foreach (var item in items) using(item)
            {
                var name = item.DatabaseName;
                var shapes = item.ShapeFiles;
                item.CreateDatabase();
                item.Import();

                if (item.DatabaseName == "test_mn_import")
                {
                    AddSpatialColumn("geom", 4326, "Multipolygon", "easements", "localhost", "postgres", item.DatabaseName);
                    AddSpatialColumn("geom", 3857, "Multipolygon", "easements", "localhost", "postgres", item.DatabaseName);

                    AddSpatialColumn("geom", 4326, "Multipolygon", "lakes", "localhost", "postgres", item.DatabaseName);
                    AddSpatialColumn("geom", 3857, "Multipolygon", "lakes", "localhost", "postgres", item.DatabaseName);


                    AddSpatialColumn("geom", 4326, "Multipolygon", "state_parks", "localhost", "postgres", item.DatabaseName);
                    AddSpatialColumn("geom", 3857, "Multipolygon", "state_parks", "localhost", "postgres", item.DatabaseName);

                    AddSpatialColumn("geom", 4326, "Multipolygon", "stream_regs", "localhost", "postgres", item.DatabaseName);
                    AddSpatialColumn("geom", 3857, "Multipolygon", "stream_regs", "localhost", "postgres", item.DatabaseName);

                    AddSpatialColumn("geom", 4326, "Multipolygon", "wma", "localhost", "postgres", item.DatabaseName);
                    AddSpatialColumn("geom", 3857, "Multipolygon", "wma", "localhost", "postgres", item.DatabaseName);

                    AddSpatialColumn("geom", 4326, "Multipolygon", "trout_lakes", "localhost", "postgres", item.DatabaseName);
                    AddSpatialColumn("geom", 3857, "Multipolygon", "trout_lakes", "localhost", "postgres", item.DatabaseName);

                    DoStuff("streams", "926915", "localhost", "postgres", "test_mn_import", "geom");
                    DoStuff("trout_streams", "926915", "localhost", "postgres", "test_mn_import", "geom");
                }
            }
        }

        protected const string CleanedSpatialColumn = "geom_2d";

        protected const string AlterTableToMultiLineString = @"ALTER TABLE {0} ADD {1} geometry(MultiLineString, {2})";
        protected const string NonUniqueColumnBstarIndex = @"CREATE UNIQUE INDEX ix_{0}_{1} ON public.{0} USING btree (gid ASC NULLS LAST); ALTER TABLE public.{0} CLUSTER ON ix_{0}_{1};";  // table name, column name
        protected const string UpdateMultilineColumn = @"UPDATE {0} SET {1} = ST_Force_2D({2})";
        protected const string MultilineString = "MultiLineString";

        protected virtual void DoStuff(string _tableName, string _shapefileSrid, string _hostName, string _userName, string _databaseName, string OriginalSpatialColumn )
        {
            var alterScript = String.Format(AlterTableToMultiLineString, _tableName, CleanedSpatialColumn, _shapefileSrid);

            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, alterScript);
            ExecuteShellCommand.ExecuteProcess(alterCommand);

            var updateTableScript = String.Format(UpdateMultilineColumn, _tableName, CleanedSpatialColumn, OriginalSpatialColumn);
            var updateCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, updateTableScript);
            ExecuteShellCommand.ExecuteProcess(updateCommand);
            ApplyNonUniqueIndexToColumn("kittle_nbr", _tableName, _hostName, _userName, _databaseName);

            AddSpatialColumn(CleanedSpatialColumn, 4326, MultilineString, _tableName, _hostName, _userName, _databaseName);
            AddSpatialColumn(CleanedSpatialColumn, 3857, MultilineString, _tableName, _hostName, _userName, _databaseName);
        }

        protected void ApplyNonUniqueIndexToColumn(string columnName, string _tableName, string _hostName, string _userName, string _databaseName)
        {
            var alterTableScript = String.Format(NonUniqueColumnBstarIndex, _tableName, columnName);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", _hostName,
                _userName, _databaseName, alterTableScript);
            ExecuteShellCommand.ExecuteProcess(alterCommand);
        }

        protected virtual void AddSpatialColumn(string geometryColumnName, int desiredSrid, string geometryType, string _tableName, string _hostName, string _userName, string _databaseName)
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
    }
}
