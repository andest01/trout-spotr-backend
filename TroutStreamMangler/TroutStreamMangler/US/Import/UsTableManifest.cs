using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.US.Import
{
    public class UsTableManifest : ITableImporterManifest
    {
        private readonly DirectoryInfo _rootDirectory;
        private const string OriginalSrid = "4269";
        private readonly IDbConnection _connection;
        private readonly Lazy<IEnumerable<ITableImporter>> _tables;
        public UsTableManifest(DirectoryInfo rootDirectory, IDbConnection connection)
        {
            _rootDirectory = rootDirectory;
            _connection = connection;
            _tables = new Lazy<IEnumerable<ITableImporter>>(CreateTables);
        }

        private IEnumerable<ITableImporter> CreateTables()
        {
            var countyDirectory = MoveTo("Counties");
            yield return
                new UsCountyImporter(countyDirectory.FullName, _connection, "counties",
                    OriginalSrid);

            var soughtDirectory = MoveTo("States");
            yield return
                new UsStateImporter(soughtDirectory.FullName, _connection, "states",
                    OriginalSrid);
        }

        public IEnumerable<ITableImporter> TableImporters
        {
            get { return _tables.Value; }
        }

        private DirectoryInfo MoveTo(string soughtPath)
        {
            var soughtDirectory = new DirectoryInfo(_rootDirectory.FullName + @"\" + soughtPath);
            return soughtDirectory;
        }
    }
}
