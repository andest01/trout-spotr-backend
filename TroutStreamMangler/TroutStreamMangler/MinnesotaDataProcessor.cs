using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroutDash.DatabaseImporter;
using TroutStreamMangler.MN;

namespace TroutStreamMangler
{
    public interface IDataProcessor
    {
        void Process();
    }

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

    public class MinnesotaDataProcessor : DataProcessor
    {
        public MinnesotaDataProcessor(IDatabaseImporter importer, IDatabaseExporter exporter) : base(importer, exporter)
        {
        }
    }
}
