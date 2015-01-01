using System;
using System.IO;

namespace TroutDash.DatabaseImporter.Convention.DataImporter
{
    public interface IDataPackager : IDisposable
    {
        DirectoryInfo Unpack();
        FileInfo Pack();
    }
}