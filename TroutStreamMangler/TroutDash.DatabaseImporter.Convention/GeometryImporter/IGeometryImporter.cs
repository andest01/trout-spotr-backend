using System;
using System.IO;

namespace TroutDash.DatabaseImporter.Convention.GeometryImporter
{
    public interface IGeometryImporter : IDisposable
    {
        void ImportShape(FileInfo shapefile, IDatabaseConnection connection);
    }

    public interface IGeometryFileImporter : IDisposable
    {
        FileInfo File { get; }
        String SRID { get; }
        void ImportFile();
    }

    public class GeometryShapeFileImporter : IGeometryFileImporter
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public FileInfo File { get; private set; }
        public string SRID { get; private set; }
        public void ImportFile()
        {
            throw new NotImplementedException();
        }
    }
}