using System;
using System.Collections.Generic;
using System.IO;
using ProcessorLookup = System.Collections.Generic.IDictionary<string, TroutDash.DatabaseImporter.Convention.DatabaseImporter.IDatabaseTableProcessor>;

namespace TroutDash.DatabaseImporter.Convention.DatabaseImporter
{
    public interface IDatabaseImporter : IDisposable
    {
        void CreateDatabase();
        void Import(ProcessorLookup processors = null);
        string DatabaseName { get; }
        IEnumerable<FileInfo> ShapeFiles { get; }
    }
}
