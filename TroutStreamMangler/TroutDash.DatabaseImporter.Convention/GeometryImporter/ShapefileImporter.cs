using System;
using System.IO;

namespace TroutDash.DatabaseImporter.Convention.GeometryImporter
{
    public class ShapefileImporter : IGeometryImporter
    {
        public void Dispose()
        {
            
        }

        public void ImportShape(FileInfo shapefile, IDatabaseConnection connection, string srid, string desiredSrid = null)
        {
            var sqlFile = DumpSqlFromShapefile(shapefile, srid, desiredSrid);
            try
            {
                ExecuteShellCommand.ExecuteSql(sqlFile, connection.DatabaseName, connection.HostName, connection.UserName);
            }

            finally
            {
                // granted, we have a dispose, but this was ported over from
                // older code, so I just kind of, you know, want to keep
                // moving forward.

                // TODO: Remove this finally and make this code to the Dispose() method.
                sqlFile.Delete();
            }
        }

        private FileInfo DumpSqlFromShapefile(FileInfo shapeFile, string srid, string toSrid = null)
        {
            var shortName = Path.GetFileNameWithoutExtension(shapeFile.Name);
            Console.WriteLine("Starting import for file named " + Path.GetFileNameWithoutExtension(shapeFile.Name));
            Console.WriteLine("Creating sql file for " + shapeFile);
            var sridConversion = String.IsNullOrEmpty(toSrid) ? srid : srid + ":" + toSrid;
            const string commandTemplate = "shp2pgsql -d -s {2} -I -W LATIN1 {0} {1} > {1}.sql";
            var command = String.Format(commandTemplate, shapeFile.FullName, shortName, sridConversion);
            ExecuteShellCommand.ExecuteProcess(command);

            var sqlFileName = Path.GetFileNameWithoutExtension(shapeFile.FullName) + ".sql";
            return new FileInfo(sqlFileName);
        }
    }
}