using System;
using System.IO;

namespace TroutDash.DatabaseImporter.Convention.GeometryImporter
{
    public class ShapefileImporter : IGeometryImporter
    {
        public void Dispose()
        {
            
        }

        public void ImportShape(FileInfo shapefile, IDatabaseConnection connection)
        {
            var sqlFile = DumpSqlFromShapefile(shapefile);
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

        private FileInfo DumpSqlFromShapefile(FileInfo shapeFile)
        {
            var shortName = Path.GetFileNameWithoutExtension(shapeFile.Name);
            Console.WriteLine("Starting import for file named " + Path.GetFileNameWithoutExtension(shapeFile.Name));
            Console.WriteLine("Creating sql file for " + shapeFile);
            const string commandTemplate = "shp2pgsql -d -I -W LATIN1 {0} {1} > {1}.sql";
            var command = String.Format(commandTemplate, shapeFile.FullName, shortName);
            ExecuteShellCommand.ExecuteProcess(command);

            var sqlFileName = Path.GetFileNameWithoutExtension(shapeFile.FullName) + ".sql";
            return new FileInfo(sqlFileName);
        }
    }
}