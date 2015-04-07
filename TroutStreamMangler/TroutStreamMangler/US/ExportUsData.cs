using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using Newtonsoft.Json;
using Npgsql;
using TroutDash.DatabaseImporter.Convention;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN;
using TroutStreamMangler.MN.Models;
using TroutStreamMangler.US.Models;

namespace TroutStreamMangler.US
{
    public class ExportUsData : ConsoleCommand
    {
        private readonly IDatabaseConnection _troutDashDbConnection;
        private readonly IDatabaseConnection _usImportDbConnection;
        private readonly Lazy<ILookup<int, FipsCode>> _fips;
        private readonly Lazy<Dictionary<string, Abbreviation>> _abbrevs;
        private readonly SequenceRestarter _sequenceRestarter;

        public ExportUsData(IDatabaseConnection troutDashDbConnection, IDatabaseConnection usImportDbConnection)
        {
            _troutDashDbConnection = troutDashDbConnection;
            _usImportDbConnection = usImportDbConnection;
            _sequenceRestarter = new SequenceRestarter(troutDashDbConnection);
            _abbrevs = new Lazy<Dictionary<string, Abbreviation>>(CreateAbbreviations);
            _fips = new Lazy<ILookup<int, FipsCode>>(CreateFipCode);
            HasOption("regionCsv=", "the location of the regions csv, etc", j => RegionCsv = j);
        }

        public string RegionCsv { get; set; }

        private static Dictionary<string, Abbreviation> CreateAbbreviations()
        {
            var abbreviationsFile = new FileInfo(@"US\Data\US_State_abbreviations.json");
            return JsonConvert.DeserializeObject<IEnumerable<Abbreviation>>(File.ReadAllText(abbreviationsFile.FullName))
                .ToDictionary(i => i.State_name);
        }

        private ILookup<int, FipsCode> CreateFipCode()
        {
            var fipsCodesFile = new FileInfo(@"US\Data\US_FIPS_CODES.json");

            var fipsCodes = JsonConvert.DeserializeObject<IEnumerable<CensusFipsCodes>>(
                File.ReadAllText(fipsCodesFile.FullName))
                .Select(OnSelector).ToLookup(i => i.StateNumber);

            return fipsCodes;
        }

        private FipsCode OnSelector(CensusFipsCodes i)
        {
            return new FipsCode
            {
                CountyName = i.County_Name,
                CountyNumber = i.FIPS_County,
                StateName = i.State,
                StateNumber = i.FIPS_State,
                StateAbbreviation = _abbrevs.Value[i.State].State_abbreviation
            };
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                ImportStatesAndCounties();
                ImportRegions();
            }

            // update or delete on table "county" violates foreign key constraint "FK_Stream_County_County" on table "stream_county"
            catch (Exception)
            {
                throw;
            }

            return 0;
        }

        private void ImportRegions()
        {
            const string regionNamesFileName = "RegionNames.csv";
            var regionDirectory = new DirectoryInfo(RegionCsv);
            var csvs = regionDirectory.EnumerateFiles("*.csv", SearchOption.TopDirectoryOnly);

            var regionNamesFile = csvs.Single(f => f.Name == regionNamesFileName);
            var names =
                RegionsBuilder.GetCsvModel<RegionNameModel>(regionNamesFile.FullName).ToDictionary(i => i.FileName);

            var regions = csvs.Where(f => f.Name != regionNamesFileName)
                .Select(RegionsBuilder.GetRegionModel).ToList();
            Console.WriteLine("Saving regions");
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                Console.WriteLine("Deleting all regions");
                troutDashContext.regions.RemoveRange(troutDashContext.regions);
                troutDashContext.SaveChanges();
                _sequenceRestarter.RestartSequence("region_id_seq");

                Console.WriteLine("Saving region:");
                foreach (var region in regions)
                {
                    Console.WriteLine("   " + region.regionName);
                    var r = new region();
                    r.name = region.regionName;
                    var fipsCodes = region.Counties.Select(i => i.FIPS).ToList();
                    r.counties =
                        troutDashContext.counties.Where(i => fipsCodes.Any(f => f == i.statefp + i.countyfp))
                            .ToList();
                    r.Geom = DisolveCountyGeometriesByRegion(region);
                    r.long_name = names[region.regionName].FullName;
                    troutDashContext.regions.Add(r);
                    troutDashContext.SaveChanges();
                }
            }
        }

        private string DisolveCountyGeometriesByRegion(RegionModel region)
        {
            var sql = @"select * from
ST_Multi(ST_Union(ARRAY
(SELECT geom_4326
  FROM public.counties
  where CONCAT(statefp, countyfp) in ({0}))))";
            var safeFipsTemplate = "'{0}'";



            var connection = String.Format("Server={0};Port=5432;User Id={1};Database={2};Password=fakepassword;", _usImportDbConnection.HostName, _usImportDbConnection.UserName, _usImportDbConnection.DatabaseName);
            var conn = new NpgsqlConnection(connection);
            conn.Open();

            var safeCountyFips = region.Counties.Select(i => String.Format(safeFipsTemplate, i.FIPS)).ToArray();
            var args = string.Join(",", safeCountyFips);
            var query = string.Format(sql, args);
            var command = new NpgsqlCommand(query, conn);

            var dr = command.ExecuteReader();
            while (dr.Read())
            {
                var geom = dr.GetString(0);
                return geom;
            }

            return null;
        }

        private void ImportStatesAndCounties()
        {
            using (var context = new UsShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                var states = troutDashContext.states.ToList();
                troutDashContext.states.RemoveRange(states);
                troutDashContext.SaveChanges();
                _sequenceRestarter.RestartSequence("states_gid_seq");
                _sequenceRestarter.RestartSequence("counties_gid_seq");
                var counties = context.counties.ToLookup(c => c.statefp);
                foreach (var state in context.states)
                {
                    var actualState = _fips.Value[Convert.ToInt32(state.statefp)].FirstOrDefault();
                    if (actualState == null)
                    {
                        continue;
                    }

                    Console.WriteLine("Adding state " + actualState.StateName);
                    var stateCounties = counties[state.statefp];
                    var countyModels = stateCounties.Select(c => new TroutDash.EntityFramework.Models.county
                    {
                        Geom = c.Geom_4326,
                        countyfp = c.countyfp,
                        statefp = c.statefp,
                        name = c.name,
                        lsad = c.lsad,
                        stream_count = 0
                    }).ToList();


                    var stateAbbreviation = actualState.StateAbbreviation;
                    var stateName = actualState.StateName;

                    var newState = new TroutDash.EntityFramework.Models.state
                    {
                        counties = countyModels,
                        Name = stateName,
                        short_name = stateAbbreviation,
                        statefp = state.statefp,
                        Geom = state.Geom_4326,
                    };

                    troutDashContext.states.Add(newState);
                    troutDashContext.SaveChanges();
                }
            }
        }
    }
}