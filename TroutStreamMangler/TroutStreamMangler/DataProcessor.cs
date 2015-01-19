using System;
using System.Collections.Generic;
using System.Linq;

namespace TroutStreamMangler
{
    public abstract class DataProcessor : IDataProcessor
    {
        protected readonly IDatabaseImporter _importer;
        protected readonly IDatabaseExporter _exporter;

        protected DataProcessor(IDatabaseImporter importer, IDatabaseExporter exporter)
        {
            _importer = importer;
            _exporter = exporter;
        }

        protected virtual void PreProcess()
        {
            
        }

        protected virtual void PostProcess()
        {

        }

        public void Process()
        {
            PreProcess();

            PostProcess();
        }
    }
}