using System;

namespace TroutDash.DatabaseImporter.Convention.DatabaseImporter
{
    public interface IDatabaseTableProcessor : IDisposable
    {
        void PreProcess();
        void Process();
        void PostProcess();
    }

    public class DatabaseTableProcessorBase : IDatabaseTableProcessor
    {
        public void Dispose()
        {
            
        }

        public void PreProcess()
        {
            
        }

        public void Process()
        {
            
        }

        public void PostProcess()
        {
            
        }
    }
}