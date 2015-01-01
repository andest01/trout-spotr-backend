using System;
using System.Collections.Generic;
using System.IO;
using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaTableManifest : ITableImporterManifest
    {
        private readonly DirectoryInfo _rootDirectory;
        private readonly string OriginalSRID = "926915";
        private readonly IDbConnection _connection;
        private readonly Lazy<IEnumerable<ITableImporter>> _tables; 
        public MinnesotaTableManifest(DirectoryInfo rootDirectory, IDbConnection connection)
        {
            _rootDirectory = rootDirectory;
            _connection = connection;
            _tables = new Lazy<IEnumerable<ITableImporter>>(CreateTables);
        }

        private IEnumerable<ITableImporter> CreateTables()
        {
            var streamRoutesDirectory = MoveTo("Streams");
            yield return
                new MinnesotaStreamRouteImporter(streamRoutesDirectory.FullName, _connection, "streams_with_measured_kittle_routes",
                    OriginalSRID);

            var soughtDirectory = MoveTo("TroutStreamSections");
            yield return new MinnesotaStreamRouteImporter(soughtDirectory.FullName, _connection, "trout_streams_minnesota", OriginalSRID);

            var lakeDirectory = MoveTo("Lakes");
            yield return new MinnesotaLakeImporter(lakeDirectory.FullName, _connection, OriginalSRID);

            var regulationsDirectory = MoveTo("Restrictions");
            yield return new MinnesotaRegulationSectionsImporter(regulationsDirectory.FullName, _connection, OriginalSRID);

            var easementsDirectory = MoveTo(@"PubliclyAccessibleLands\Easements");
            yield return new MinnesotaEasementsImporter(regulationsDirectory.FullName, _connection, OriginalSRID);

            var wmaDirectory = MoveTo(@"PubliclyAccessibleLands\WildlifeManagementAreas");
            yield return new MinnesotaWildlifeManagementAreaImporter(wmaDirectory.FullName, _connection, OriginalSRID);

            var stateParkDirectory = MoveTo(@"PubliclyAccessibleLands\StateParks");
            yield return new MinnesotaStateParksImporter(stateParkDirectory.FullName, _connection, OriginalSRID);
        }

        public IEnumerable<ITableImporter> TableImporters
        {
            get { return _tables.Value; }
        }

        protected internal DirectoryInfo MoveTo(string soughtPath)
        {
            var soughtDirectory = new DirectoryInfo(_rootDirectory.FullName + @"\" + soughtPath);
            return soughtDirectory;
        }
    }
}