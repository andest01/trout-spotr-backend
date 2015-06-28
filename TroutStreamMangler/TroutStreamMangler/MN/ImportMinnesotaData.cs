using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using ManyConsole;
using TroutDash.DatabaseImporter;

namespace TroutStreamMangler
{
    public abstract class ImportMultilineStringData : AdministrativeImporterBase
    {
        protected const string AlterTableToMultiLineString = @"ALTER TABLE {0} ADD {1} geometry(MultiLineString, {2})";
        protected const string UpdateMultilineColumn = @"UPDATE {0} SET {1} = ST_Force_2D({2})";
        protected const string CleanedSpatialColumn = "geom_2d";
        protected const string OriginalSpatialColumn = "geom";
        protected readonly string _tableName;
        protected readonly string _directoryName;
        protected readonly string _originalSrid;
        protected readonly IEnumerable<string> _desiredSriDs;
        protected readonly bool _removeMeasureValues;

        protected override void PreOnRun()
        {
            
        }

        protected override void PostOnRun()
        {
            var alterScript = String.Format(AlterTableToMultiLineString, _tableName, CleanedSpatialColumn, Srid);

            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, alterScript);
            ExecuteProcess(alterCommand);

            var updateTableScript = String.Format(UpdateMultilineColumn, _tableName, CleanedSpatialColumn, OriginalSpatialColumn);
            var updateCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, updateTableScript);
            ExecuteProcess(updateCommand);

            AddSpatialColumn(_tableName, CleanedSpatialColumn, 4326, MultilineString);
//            AddSpatialColumn(_tableName, CleanedSpatialColumn, ImportShapefile.PreferredSrid, MultilineString);

            Console.WriteLine("Adding index...");
            ApplyNonUniqueIndexToColumn(_tableName, "kittle_nbr");
        }

        protected ImportMultilineStringData(string tableName, string directoryName, string originalSrid, IEnumerable<string> desiredSRIDs, bool removeMeasureValues = false)
        {
            _tableName = tableName;
            _directoryName = directoryName;
            _originalSrid = originalSrid;
            _desiredSriDs = desiredSRIDs;
            _removeMeasureValues = removeMeasureValues;
        }

        protected void Run()
        {
            PreOnRun();
            var soughtDirectory = MoveTo(_directoryName);
            Import(soughtDirectory);
            PostOnRun();
        }
    }

    public class ImportMinnesotaData : AdministrativeImporterBase
    {
        private const string SRID = "926915";
        private const string UpdateMultilineColumn = @"UPDATE {0} SET {1} = ST_Force_2D({2})";
        private const string AlterTableToMultiLineString = @"ALTER TABLE {0} ADD {1} geometry(MultiLineString, {2})";

        private const string TroutStreamsTableName = @"trout_streams_minnesota";
        private const string StreamsTableName = @"streams_with_measured_kittle_routes";
        private const string LakeTableName = @"dnr_hydro_features_all";
        private const string TroutLakeTableName = @"dnr_hydrography_stream_trout_lakes";
        private const string StateParkTableName = @"state_forest_management_units";
        private const string RoadsTableName = @"STREETS_LOAD";
        private const string OriginalSpatialColumn = "geom";
        private const string CleanedSpatialColumn = "geom_2d";



        public ImportMinnesotaData()
        {
            IsCommand("ImportMn", "Loads Minnesota Data. Takes about 5 minutes.");
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
                Console.WriteLine("Initializing MN Database...");
//                InitializeDatabase();
                Console.WriteLine("Done initializing US Database.");
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
                Console.WriteLine("Importing trout stream sections...");
                ImportTroutStreamSections();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            try
            {
                Console.WriteLine("Importing lakes...");
                ImportLakes();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            try
            {
                Console.WriteLine("Importing regulation sections...");
                ImportRegulationSections();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            try
            {
                Console.WriteLine("Importing easements...");
                ImportEasements();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }


            try
            {
                Console.WriteLine("Importing wildlife management areas...");
                ImportWildlifeManagementAreas();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            try
            {
                Console.WriteLine("Importing state parks...");
                ImportStateParks();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            try
            {
                Console.WriteLine("Importing state forests...");
                ImportStateForests();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            try
            {
                Console.WriteLine("Importing streams...");
                ImportStreams();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            try
            {
                Console.WriteLine("Importing Trout Lakes...");
                ImportTroutLakes();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            try
            {
                Console.WriteLine("Importing Roads...");
                ImportRoads();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured.");
            }

            //ImportTroutLakes

            return 0;
        }

        private void ImportStateForests()
        {
            // state_forest_management_units
//            var soughtDirectory = MoveTo(@"PubliclyAccessibleLands\StateForests");
//            Import(soughtDirectory);
            TrimGeometry(StateParkTableName);
            AddSpatialColumn(StateParkTableName, OriginalSpatialColumn, 4326, "Multipolygon");
            //            AddSpatialColumn(s, OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }

        private void ImportRoads()
        {
            var alterScript = String.Format(AlterTableToMultiLineString, RoadsTableName, CleanedSpatialColumn, "26915");

            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, alterScript);
            ExecuteProcess(alterCommand);

            var updateTableScript = String.Format(UpdateMultilineColumn, RoadsTableName, CleanedSpatialColumn, OriginalSpatialColumn);
            var updateCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, updateTableScript);
            ExecuteProcess(updateCommand);
            AddSpatialColumn(RoadsTableName, CleanedSpatialColumn, 4326, MultilineString);
        }

        private void ImportStreams()
        {
            var soughtDirectory = MoveTo("Streams");
            Import(soughtDirectory);

            var alterScript = String.Format(AlterTableToMultiLineString, StreamsTableName, CleanedSpatialColumn, SRID);

            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, alterScript);
            ExecuteProcess(alterCommand);


            var updateTableScript = String.Format(UpdateMultilineColumn, StreamsTableName, CleanedSpatialColumn, OriginalSpatialColumn);
            var updateCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, updateTableScript);
            ExecuteProcess(updateCommand);

            AddSpatialColumn(StreamsTableName, CleanedSpatialColumn, 4326, MultilineString);
//            AddSpatialColumn(StreamsTableName, CleanedSpatialColumn, ImportShapefile.PreferredSrid, MultilineString);

            Console.WriteLine("Adding index...");
            ApplyNonUniqueIndexToColumn(StreamsTableName, "kittle_nbr");
        }

        private void ImportTroutStreamSections()
        {
            var soughtDirectory = MoveTo("TroutStreamSections");
            Import(soughtDirectory);

            var alterScript = String.Format(AlterTableToMultiLineString, TroutStreamsTableName, CleanedSpatialColumn, SRID);

            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, alterScript);
            ExecuteProcess(alterCommand);

            var updateTableScript = String.Format(UpdateMultilineColumn, TroutStreamsTableName, CleanedSpatialColumn, OriginalSpatialColumn);
            var updateCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, updateTableScript);
            ExecuteProcess(updateCommand);

            AddSpatialColumn(TroutStreamsTableName, CleanedSpatialColumn, 4326, MultilineString);
//            AddSpatialColumn(TroutStreamsTableName, CleanedSpatialColumn, ImportShapefile.PreferredSrid, MultilineString);

            Console.WriteLine("Adding index...");
            ApplyNonUniqueIndexToColumn(TroutStreamsTableName, "kittle_nbr");

        }

        private void ImportLakes()
        {
            TrimGeometry(LakeTableName);
            AddSpatialColumn(LakeTableName, OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(LakeTableName, OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }

        private void ImportTroutLakes()
        {
            TrimGeometry(TroutLakeTableName);  
            AddSpatialColumn(TroutLakeTableName, OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(TroutLakeTableName, OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
            
        }

        private void ImportRegulationSections()
        {
            var s = "strm_regsln3";
            AddSpatialColumn(s, OriginalSpatialColumn, 4326, MultilineString);
//            AddSpatialColumn(s, OriginalSpatialColumn, ImportShapefile.PreferredSrid, MultilineString);
        }

        private void ImportEasements()
        {
            var s = "mndnr_fisheries_acquisition";
            TrimGeometry(s);
            AddSpatialColumn(s, OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(s, OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }

        private void ImportWildlifeManagementAreas()
        {
            var s = "dnr_wma_boundaries_pa";
            TrimGeometry(s);
            AddSpatialColumn(s, OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(s, OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
            
        }

        protected virtual void TrimGeometry(string tableName)
        {
            const string sql =
                @"delete from public.{0} where gid not in ( SELECT p.gid FROM public.{0} p, streams_with_measured_kittle_routes sk, trout_streams_minnesota t where t.trout_flag = 1 and sk.kittle_nbr = t.kittle_nbr and sk.kittle_nbr is not null and ST_Intersects(ST_Envelope(sk.geom), p.geom))";

            var alterTableScript = String.Format(sql, tableName);
            var alterCommand = String.Format(@"psql -q  --host={0} --username={1} -d {2} --command ""{3}""", HostName,
                UserName, DatabaseName, alterTableScript);
            ExecuteShellCommand.ExecuteProcess(alterCommand);
        }

        private void ImportStateParks()
        {
            var soughtDirectory = MoveTo(@"PubliclyAccessibleLands\StateParks");
            Import(soughtDirectory);
            var s = "dnr_stat_plan_areas_prk";
            TrimGeometry(s);
            AddSpatialColumn(s, OriginalSpatialColumn, 4326, "Multipolygon");
//            AddSpatialColumn(s, OriginalSpatialColumn, ImportShapefile.PreferredSrid, "Multipolygon");
        }

        protected internal override FileInfo OnAddSpacialReferenceSystem()
        {
            var file =
                new FileInfo(
                    @"C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\SridImport.sql");

            return file;
        }
    }
}