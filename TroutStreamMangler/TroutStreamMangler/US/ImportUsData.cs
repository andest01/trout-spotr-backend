using System;
using System.IO;
using TroutStreamMangler.MN;

namespace TroutStreamMangler.US
{
    public class ImportUsData : AdministrativeImporterBase
    {
        private const string SRID = "4269";
        private const string OriginalSpatialColumn = "geom";

        public ImportUsData()
        {
            IsCommand("ImportUs", "Loads Us Data, such as states and counties. Takes about 5 minutes.");
            HasRequiredOption("rootDirectory", "root directory of data", s => { RootDirectory = new DirectoryInfo(s); });
            HasRequiredOption("databaseName=", "Required database name", s => { DatabaseName = s; });
            HasRequiredOption("hostName=", "Required host name (e.g. localhost)", s => { HostName = s; });
            HasRequiredOption("username=", "Required user name (e.g. postgres or admin)", s => { UserName = s; });
        }

        protected internal override string Srid
        {
            get { return SRID; }
        }

        protected internal override int OnRun(string[] remainingArguments)
        {
            try
            {
                Console.WriteLine("Initializing US Database...");
//                InitializeDatabase();
                Console.WriteLine("Done initializing US Database.");
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
                Console.WriteLine("Importing counties...");
                ImportCounties();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while writting counties. " + e.Message);
            }

            try
            {
                Console.WriteLine("Importing states...");
                ImportStates();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while writting states. " + e.Message);
            }

            return 0;
        }

        private void ImportCounties()
        {
            var s = "counties";
            AddSpatialColumn(s, OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(s, OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }

        private void ImportStates()
        {
            var s = "states";
            AddSpatialColumn(s, OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(s, OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }
    }
}