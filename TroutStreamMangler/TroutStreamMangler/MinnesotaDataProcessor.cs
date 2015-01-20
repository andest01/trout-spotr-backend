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
    public class MinnesotaDataProcessor : DataProcessor
    {
        public MinnesotaDataProcessor(IDatabaseImporter importer, IDatabaseExporter exporter) : base(importer, exporter)
        {
        }
    }
}
