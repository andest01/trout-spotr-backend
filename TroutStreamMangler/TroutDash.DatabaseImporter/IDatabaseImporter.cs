using System;

namespace TroutDash.DatabaseImporter
{
    public interface IDatabaseImporter : IDisposable
    {
        void InitializeDatabase();
        void ImportTables();
    }
}