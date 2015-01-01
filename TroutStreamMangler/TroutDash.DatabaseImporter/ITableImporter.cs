using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace TroutDash.DatabaseImporter
{
    public interface ITableImporterManifest
    {
        IEnumerable<ITableImporter> TableImporters { get; } 
    }

    public interface ITableImporter : IDisposable
    {
        void ImportTable();
    }
}