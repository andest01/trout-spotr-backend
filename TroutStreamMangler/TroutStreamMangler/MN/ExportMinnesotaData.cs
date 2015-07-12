using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using GeoAPI.Geometries;
using ManyConsole;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Npgsql;
using TroudDash.GIS;
using TroutDash.DatabaseImporter.Convention;
using TroutDash.EntityFramework.Models;
using TroutDash.Export;
using TroutStreamMangler.MN.Models;
using TroutStreamMangler.US;

namespace TroutStreamMangler.MN
{
    public class ExportMinnesotaData : ConsoleCommand, IDatabaseExporter
    {
        private readonly IDatabaseConnection _dbConnection;
        private readonly IDatabaseConnection _mnImportDbConnection;
        private readonly CentroidResetter _centroidResetter;
        private readonly GetLinearOffsets _getLinearOffsets;
        private readonly SequenceRestarter _sequenceRestarter;
        public const decimal METERS_IN_MILE = 1609.34M;

        public class MnRoadType
        {
            // type, name, isFederal, stateId
            public string abc { get; set; }
            public string name { get; set; }
            public bool isFederal { get; set; }
            public string stateId { get; set; }
            public int road_type_id { get; set; }
        }

        public ExportMinnesotaData(IDatabaseConnection dbConnection, IDatabaseConnection mnImportDbConnection,
            CentroidResetter centroidResetter)
        {
            _dbConnection = dbConnection;
            _mnImportDbConnection = mnImportDbConnection;
            _centroidResetter = centroidResetter;
            _sequenceRestarter = new SequenceRestarter(dbConnection);
            IsCommand("ExportMn", "Exports minnesota data into trout dash. must be run after ImportMn.");
            HasRequiredOption("regulationJson=", "the location of the regulation json", j => RegulationsFileLocation = j);
            HasRequiredOption("roadTypesJson=", "the location of the road types json", j => RoadTypesFileLocation = j);
            HasOption("streamMetadata=", "the location of the stream data for species, status, etc",
                j => StreamMetadataFileLocation = j);

            _getLinearOffsets = new GetLinearOffsets(mnImportDbConnection);
            _sequenceRestarter = new SequenceRestarter(dbConnection);
        }

        public string RoadTypesFileLocation { get; set; }


        public string RegulationsFileLocation { get; set; }
        public string StreamMetadataFileLocation { get; set; }

        public override int Run(string[] remainingArguments)
        {
            // ORDER MATTERS!
//            var lakeExporter = new LakeExporter(new TroutDashPrototypeContext(), new MinnesotaShapeDataContext(),
//                new SequenceRestarter(_dbConnection), new GetLinearOffsets(_mnImportDbConnection));
//            var regulationsExporter = new RegulationsExporter(new TroutDashPrototypeContext(),
//                new MinnesotaShapeDataContext(), new SequenceRestarter(_dbConnection),
//                new GetLinearOffsets(_mnImportDbConnection));
//            var streamAccessPointsExporter = new StreamAccessPointsExporter(new TroutDashPrototypeContext(),
//                new MinnesotaShapeDataContext(), new SequenceRestarter(_dbConnection),
//                new GetLinearOffsets(_mnImportDbConnection));
//
//            ExportRestrictions();
//            ExportMnRoadTypes();
//            ExportStreams();
//            ExportMnRoads();
//
//
//            lakeExporter.ExportLakes();
//
//            // EXPORT RELATIONSHIPS AFTER ENTITIES HAVE BEEN LOADED.
//            ExportRestrictionRoutes();
//            regulationsExporter.ExportRestrictionSections();
//            ExportPubliclyAccessibleLand();
//            lakeExporter.ExportLakeSections();
//            ExportCountyToStreamRelations();
//            ExportStreamToPubliclyAccessibleLandRelations();
//            ExportPubliclyAccessibleLandSections();
//            
//            
            ExportStreamAccessPoints();

            return 0;
            }

        private void ExportMnRoads()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                const int UncategorizedRoad = 100;
                
                // Remove old MN road types.
                Console.WriteLine("Deleting Minnesota roads...");
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                troutDashContext.roads.RemoveRange(minnesota.roads);
                troutDashContext.SaveChanges();
                Console.WriteLine("Done deleting Minnesota roads.");

                Console.WriteLine("Getting Minnesota roads...");
                var roadTypes = minnesota.road_types.ToDictionary(i => i.source);

                var roads = context.streets_load.ToList().Select(i =>
                {
                    var key = Convert.ToInt32(i.route_syst).ToString();
                    var roadTypeId = -1;
                    if (i.route_syst == "1")
                    {
                        roadTypeId = 1;
                    }

                    else if (i.route_syst == "2")
                    {
                        roadTypeId = 2;
                    }
                    else {
                        roadTypeId = roadTypes.ContainsKey(key) == false
                        ? roadTypes.Single(j => j.Value.source == UncategorizedRoad.ToString()).Value.id : 
                        roadTypes[key].id;
                    }

                    var t = new road()
                    {
                        name = i.street_nam,
                        local_name = "",
                        state_gid = minnesota.gid,
                        road_type_id = roadTypeId,
                        Geom = i.Geom_4326
                    };

                    return t;
                }).ToList();

                Console.WriteLine("Saving Minnesota roads...");
                troutDashContext.roads.AddRange(roads);
                troutDashContext.SaveChanges();
                Console.WriteLine("Done saving Minnesota roads.");

            }
        }

        private void ExportMnRoadTypes()
        {
            
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                // Remove old MN road types.
                Console.WriteLine("Deleting Minnesota road types...");
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                troutDashContext.road_types.RemoveRange(minnesota.road_types.ToList());
                troutDashContext.SaveChanges();
                Console.WriteLine("Done deleting Minnesota road types.");

                Console.WriteLine("Getting Minnesota road types...");
                var roadTypes = GetMnRoadTypes().ToList().Select(i => new road_type()
                {
                    description = i.name,
                    state_gid = minnesota.gid,
                    source = i.road_type_id.ToString(),
                    type = i.abc,
                });

                Console.WriteLine("Saving Minnesota road types...");
                troutDashContext.road_types.AddRange(roadTypes);
                troutDashContext.SaveChanges();
                Console.WriteLine("Done saving Minnesota road types.");

            }
        }

        private IEnumerable<MnRoadType> GetMnRoadTypes()
        {
            using (var textStream = File.OpenText(RoadTypesFileLocation))
            {
                var csv = new CsvReader(textStream);
                while (csv.Read())
                {
                    var record = csv.GetRecord<MnRoadType>();
                    yield return record;
                }
            }
        }

        private void ExportStreamAccessPoints()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                Console.WriteLine("Deleting minnesota stream access points...");
                var state = troutDashContext.states.First(s => s.short_name == "MN");
                var pre_existing_sections = state.streams.SelectMany(s => s.stream_access_points).ToList();
                troutDashContext.stream_access_points.RemoveRange(pre_existing_sections);
                troutDashContext.SaveChanges();

                var roadIntersections = GetRoadStreamIntersections().ToList();
                troutDashContext.stream_access_points.AddRange(roadIntersections);
                troutDashContext.SaveChanges();
            }
        }

        private IEnumerable<stream_access_point> GetRoadStreamIntersections()
        {
            const string query =
@"SELECT
  subquery.road_gid,
  subquery.stream_gid,
  subquery.stream_name,
  subquery.road_name,
  ST_X((subquery.intersection_dump).geom) AS latitude,
  ST_Y((subquery.intersection_dump).geom) AS longitude,
  St_linelocatepoint(subquery.mini_line, (subquery.intersection_dump).geom) AS offset,
  (subquery.intersection_dump).geom,
  geometrytype((subquery.intersection_dump).geom) AS geometry_type,
  ST_DWithin(pal.geom, (subquery.intersection_dump).geom, 0.0008) as is_within_land,
  row_number() over () as fake_id
  
FROM (SELECT
  road.gid AS road_gid,
  stream.gid AS stream_gid,
  stream.name AS stream_name,
  road.name AS road_name,
  ST_Dump(ST_Intersection(stream.geom, road.geom)) AS intersection_dump,
  ST_linemerge(stream.geom) AS mini_line

FROM stream,
     road

WHERE ST_Intersects(stream.geom, road.geom)) AS subquery
left join publicly_accessible_land as pal 
on ST_DWithin(pal.geom, (subquery.intersection_dump).geom, 0.0008)";
            return DoStuff(query);

        }

        private IEnumerable<stream_access_point> DoStuff(string sql)
        {
            var connection = String.Format("Server={0};Port=5432;User Id={1};Database={2};CommandTimeout=60;Password=fakepassword;Timeout=60;", _dbConnection.HostName, _dbConnection.UserName, _dbConnection.DatabaseName);
            var conn = new NpgsqlConnection(connection);
            conn.Open();

            NpgsqlCommand command = new NpgsqlCommand(sql, conn);


            try
            {
                NpgsqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    var l = new stream_access_point();

                    // sometimes when a line intercepts a polygon you can
                    // get a point, e.g. the EXACT mouth of a river.
                    // ignore these non-line geometries.
                    l.is_accessible = false;
                    l.linear_offset = 0.0M;
                    l.road_gid = dr.GetInt32(0);
                    l.stream_gid = dr.GetInt32(1);
                    l.street_name = dr.GetString(3);
                    
                    l.centroid_latitude = Convert.ToDecimal(dr.GetDouble(5));
                    l.centroid_longitude = Convert.ToDecimal(dr.GetDouble(4));
                    l.linear_offset = Convert.ToDecimal(dr.GetDouble(6));
                    var asdf = dr.GetFieldDbType(9);
                    var isAccessible = dr.IsDBNull(9) == false ? dr.GetBoolean(9) : false;
                    l.is_accessible = isAccessible;
                    l.Geom = dr.GetString(7);

//                    l.IntersectionGeometryType = dr.GetString(6);
//                    var isLineString = String.Equals("Linestring", l.IntersectionGeometryType,
//                        StringComparison.OrdinalIgnoreCase);
//                    if (isLineString == false)
//                    {
//                        yield break;
//                    }
//
//                    if (dr.IsDBNull(1))
//                    {
//                        yield break;
//                    }
//
//                    l.StreamId = dr.GetInt32(0);
//                    l.StreamNumber = dr.IsDBNull(1) ? "Unnamed" : dr.GetString(1);
//                    l.StreamName = dr.IsDBNull(2) ? "" : dr.GetString(2);
//                    l.GeometryName = dr.IsDBNull(3) ? "" : dr.GetString(3);
//                    var asdf = dr[4];
//                    l.GeometryId = Convert.ToInt32(asdf);
//
//                    l.IntersectionGeometry = dr.GetString(5);
//
//
//                    l.StartPoint = dr.GetDouble(7);
//                    l.EndPoint = dr.GetDouble(8);
//                    l.StreamLength = dr.GetDouble(9);

                    yield return l;
                }
            }

            finally
            {
                conn.Close();
            }
        }


        private void ExportRestrictionRoutes()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {

                // clear out the old.
                Console.WriteLine("Caching minnesota restriction routes...");
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                var routes = minnesota.restrictions.SelectMany(i => i.restrictionRoutes);
                troutDashContext.restriction_routes.RemoveRange(routes);
                Console.WriteLine("Deleting minnesota restriction routes...");
                troutDashContext.SaveChanges();
                _sequenceRestarter.RestartSequence("restriction_route_id_seq");

                var restrictions = troutDashContext.restrictions.ToDictionary(i => i.source_id);
                var restrictionRoutes = context.strm_regsln3.ToList();
                Console.WriteLine("Building minnesota restriction routes...");
                foreach (var restrictionRoute in restrictionRoutes)
                {
                    var route = new restriction_route();
                    route.Restriction = restrictions[restrictionRoute.new_reg.ToString()];
                    route.Geom = restrictionRoute.Geom_4326;
                    route.source_id = restrictionRoute.gid.ToString();
                    troutDashContext.restriction_routes.Add(route);
                }
                Console.WriteLine("Saving minnesota restriction routes...");
                troutDashContext.SaveChanges(); // TODO: MOVE THIS.
                Console.WriteLine("Saved minnesota restriction routes...");
            }

            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                var stuff =
                    troutDashContext.restrictions.SelectMany(r => r.restrictionRoutes).Select(i => i.gid).ToList();
            }
        }

        private void ExportStreamToPubliclyAccessibleLandRelations()
        {
            List<publicly_accessible_land> mnPals = new List<publicly_accessible_land>();
            List<stream> mnStreams = new List<stream>();
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                Console.WriteLine("Caching minnesota counties and streams...");
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                mnPals = minnesota.Pal.ToList();
                mnStreams = minnesota.streams.ToList();
            }


            Console.WriteLine("Finding all streams in each county");
            mnStreams.AsParallel()
                .ForAll(stream =>
                {
                    using (var troutDashContext2 = new TroutDashPrototypeContext())
                    {
                        try
                        {
                            Console.WriteLine(stream.name + " stream...");
                            foreach (var mnPal in mnPals)
                            {
                                var isMatch = mnPal.OriginalGeometry.Intersects(stream.OriginalGeometry);
                                if (isMatch)
                                {
                                    Console.WriteLine("   has " + mnPal.area_name);
                                    mnPal.streams.Add(stream);
                                }
                            }

                            troutDashContext2.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
                );
        }

        public void ExportRestrictions()
        {
            var restrictions = GetRestrictions();
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                var states = troutDashContext.counties.ToList();
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");

                // clear out the old.
                Console.WriteLine("Removing all restrictions in minnesota");
                var oldRestrictions = minnesota.restrictions.ToList();
                troutDashContext.restrictions.RemoveRange(oldRestrictions);
                troutDashContext.SaveChanges();

                _sequenceRestarter.RestartSequence("restriction_id_seq");

                // in with the new.
                var newRestrictions = restrictions.Select(r => new restriction
                {
                    state = minnesota,
                    isAnglingRestriction = r.isAnglingRestriction,
                    isHarvestRestriciton = r.isHarvestRestriction,
                    isSeasonal = false,
                    short_text = r.shortText,
                    legal_text = r.text,
                    source_id = r.id.ToString(),
                }).ToList();

                Console.WriteLine("Saving new restrictions");
                troutDashContext.restrictions.AddRange(newRestrictions);
                troutDashContext.SaveChanges();
            }
        }

        private IEnumerable<MinnesotaRestriction> GetRestrictions()
        {
            var file = File.ReadAllText(RegulationsFileLocation);
            var results = JsonConvert.DeserializeObject<MinnesotaRestriction[]>(file);
            return results;
        }



        public void ExportStreams()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                Console.WriteLine("Deleting all streams in Minnesota");
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                minnesota.streams.ToList().ForEach(s => troutDashContext.streams.Remove(s));
                troutDashContext.SaveChanges();
                _sequenceRestarter.RestartSequence("stream_gid_seq");
                _sequenceRestarter.RestartSequence("trout_stream_section_gid_seq");
                Console.WriteLine("Deleted all streams in Minnesota");

                // gather ye trout streams.
                ILookup<string, trout_streams_minnesota> troutStreamSectionIds =
                    context.trout_streams_minnesota.Where(ts => ts.trout_flag == 1).ToLookup(tss => tss.kittle_nbr);

                var badList = new List<trout_streams_minnesota>();

                troutStreamSectionIds.AsParallel().ForAll(section => SaveStream(section, minnesota.Name, minnesota.gid, badList));

                Console.WriteLine("Failures: ");
                badList.ForEach(b => Console.WriteLine(b.kittle_nam + " " + b.kittle_nbr));
                troutDashContext.SaveChanges();
            }

            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                Console.WriteLine("Saving Metadata for stream... ");
                using (var writer = File.OpenText(StreamMetadataFileLocation))
                {
                    var csv = new CsvReader(writer);
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<StreamSpeciesCsv>();
                        var id = record.Id;
                        try
                        {
                            var stream = minnesota.streams.FirstOrDefault(s => s.source == id);
                            if (stream == null)
                            {
                                continue;
                            }
                            Console.WriteLine("  " + stream.name + " | " + stream.source);
                            stream.has_brook_trout = record.IsBrook ?? false;
                            stream.has_brown_trout = record.IsBrown ?? false;
                            stream.has_rainbow_trout = record.IsRainbow ?? false;

                            stream.is_brook_trout_stocked = record.IsBrookStocked ?? false;
                            stream.is_brown_trout_stocked = record.IsBrownStocked ?? false;
                            stream.is_rainbow_trout_stocked = record.IsRainbowStocked ?? false;
                            stream.status_mes = record.Status ?? String.Empty;

                            stream.local_name = record.AltName ?? String.Empty;

                            troutDashContext.SaveChanges();
                        }

                        catch
                        {
                            throw;
                        }
                        
                    }
                }

                Console.WriteLine("updating stream centroids...");
                _centroidResetter.RestartSequence();
            }
        }

        private void SaveStream(IGrouping<string, trout_streams_minnesota> section, string stateName, int stateId, List<trout_streams_minnesota> badList)
        {

                using (var context = new MinnesotaShapeDataContext())
                using (var troutDashContext = new TroutDashPrototypeContext())
                {
                    try
                    {
                        var id = section.Key;
                        Console.WriteLine("Loading Trout Stream Id: " + id);
                        var route = context.StreamRoute.Single(sr => sr.kittle_nbr == id);

                        stream stream = CreateStream(route, stateName, stateId);
                        Console.WriteLine("Saving trout stream: " + stream.name);
                        troutDashContext.streams.Add(stream);
                        troutDashContext.SaveChanges();
                        CreateStreamSections(route, section, stream);
                        troutDashContext.SaveChanges();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("FAILED LOAIND STREAM: " + section.Key);
                        badList.Add(section.First());
                    }
                }
        }

        private stream CreateStream(StreamRoute route, string stateName, int stateidd)
        {
            var name = route.kittle_nam ?? "Unnamed Stream";
            Console.WriteLine("Adding stream " + name + " | " + route.kittle_nbr);
            var stream = new stream();
            var centroid = route.Geometry_4326.Centroid;
            stream.centroid_latitude = Convert.ToDecimal(centroid.X);
            stream.centroid_longitude = Convert.ToDecimal(centroid.Y);
            stream.has_brook_trout = true;
            stream.has_brown_trout = true;
            stream.has_rainbow_trout = true;
            stream.is_brook_trout_stocked = true;
            stream.is_brown_trout_stocked = true;
            stream.is_rainbow_trout_stocked = true;
            stream.length_mi = Convert.ToDecimal(route.OriginalGeometry.Length)/METERS_IN_MILE; // fix later.
            stream.local_name = route.kittle_nam;
            stream.slug = Guid.NewGuid().ToString();
            stream.source_id = route.gid.ToString();
            stream.source = route.kittle_nbr;
            stream.state_gid = stateidd;
            stream.state = stateName;
            stream.Geom = route.Geom_4326;
            stream.name = route.kittle_nam ?? "Unnamed Stream";
            stream.status_mes = "Flood";
//            stream.trout_stream_sections = trout_stream_sectionIds.SelectMany(section => CreateSection(section, route)).ToList();
            return stream;
        }

        public void CreateStreamSections(StreamRoute route,
            IEnumerable<trout_streams_minnesota> trout_stream_sectionIds, stream stream)
        {
            stream.trout_stream_sections = trout_stream_sectionIds.SelectMany(section => CreateSection(section, route)).ToList();
        }

        public void ExportPubliclyAccessibleLand()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                Console.WriteLine("Removing all publicly accessible land in minnesota");
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                var oldPal = minnesota.PalTypes.ToList();
                troutDashContext.PalTypes.RemoveRange(oldPal);
                troutDashContext.SaveChanges();

                _sequenceRestarter.RestartSequence("publicly_accessible_land_type_id_seq");
//                _sequenceRestarter.RestartSequence("publicly_accessible_land_section_id_seq");
                _sequenceRestarter.RestartSequence("publicly_accessibly_land_gid_seq");

                Console.WriteLine("Adding all ublicly accessible land in minnesota");
                minnesota.PalTypes = new List<publicly_accessible_land_type>
                {
                    new publicly_accessible_land_type
                    {
                        state = minnesota,
                        description = "Wildlife Management Area",
                        is_federal = false,
                        type = "WMA"
                    },
                    new publicly_accessible_land_type
                    {
                        state = minnesota,
                        description = "State Parks",
                        is_federal = false,
                        type = "State Park",
                    },
                    new publicly_accessible_land_type
                    {
                        state = minnesota,
                        description = "Land Easement",
                        is_federal = false,
                        type = "Easement"
                    },
                    new publicly_accessible_land_type
                    {
                        state = minnesota,
                        description = "State Forest",
                        is_federal = false,
                        type = "State Forest"
                    }
                };
                troutDashContext.SaveChanges();

                var easementType =
                    troutDashContext.PalTypes.Single(
                        i => i.state.gid == minnesota.gid && i.type == "Easement");

                var accessableEasements = context.mndnr_fisheries_acquisition
                    .Where(i => i.use_reg != mndnr_fisheries_acquisition.NoAccess)
                    .ToList() //i.use_reg_type != use_reg_types.NoAccess)
                    .Select(i => new publicly_accessible_land
                    {
                        state_gid = minnesota.gid,
                        area_name = i.unit_name,
                        pal_Id = easementType.id,
                        shape_area = Convert.ToDecimal(i.OriginalGeometry.Area),
                        Geom = i.Geom_4326
                    }).ToList();
                Console.WriteLine("Loading easements");
                troutDashContext.publicly_accessible_lands.AddRange(accessableEasements);

                var stateparkType =
                    troutDashContext.PalTypes.Single(
                        i => i.state.gid == minnesota.gid && i.type == "State Park");

                var parks = context.dnr_stat_plan_areas_prk
                    .ToList()
                    .Select(p => new publicly_accessible_land
                    {
                        state_gid = minnesota.gid,
                        area_name = p.area_name,
                        pal_Id = stateparkType.id,
                        shape_area = Convert.ToDecimal(p.OriginalGeometry.Area),
                        Geom = p.Geom_4326
                    });

                Console.WriteLine("Loading state parks");
                troutDashContext.publicly_accessible_lands.AddRange(parks);

                var wmaType = troutDashContext.PalTypes.Single(
                    i => i.state.gid == minnesota.gid && i.type == "WMA");

                var wmas = context.dnr_wildlife_management_area_boundaries_publicly_accessible
                    .ToList()
                    .Select(i => new publicly_accessible_land
                    {
                        state_gid = minnesota.gid,
                        Geom = i.Geom_4326,
                        pal_Id = wmaType.id,
                        area_name = i.unitname,
                        shape_area = Convert.ToDecimal(i.OriginalGeometry.Area)
                    });

                

                

                Console.WriteLine("Loading wildlife management areas");
                troutDashContext.publicly_accessible_lands.AddRange(wmas);

                var stateParks = context.state_forest_management_units
                    .ToList()
                    .Select(i => new publicly_accessible_land
                    {
                        state_gid = minnesota.gid,
                        Geom = i.Geom_4326,
                        pal_Id = wmaType.id,
                        area_name = i.unit_name,
                        shape_area = Convert.ToDecimal(i.OriginalGeometry.Area)
                    });
                troutDashContext.publicly_accessible_lands.AddRange(stateParks);

                Console.WriteLine("Saving publicly accessible land changes");

                troutDashContext.SaveChanges();
            }
        }

        public void ExportLakes()
        {
            
        }

        public void ExportSections()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {

                var routesWithTroutStreamSections = GetMinnesotaStreamRoutesThatHaveTroutStreamSections(context);

                
            }
        }

        public IQueryable<StreamRoute> GetMinnesotaStreamRoutesThatHaveTroutStreamSections(
            MinnesotaShapeDataContext context)
        {
            var troutStreamSections =
                context.StreamRoute.Where(
                    i => context.trout_streams_minnesota.Any(tss => tss.kittle_nbr == i.kittle_nbr));

            return troutStreamSections;
        }

        public IEnumerable<publicly_accessible_land_section> SaveEasements(publicly_accessible_land_type easement, stream route, string palType, string table)
        {
            var kittleNumber = route.source_id;
            Console.WriteLine(palType + "... ");
            var offsets = _getLinearOffsets.GetOffsets(kittleNumber, table).ToArray();
            foreach (var offset in offsets)
            {
                var section = new publicly_accessible_land_section();
                section.stream_gid = route.gid;
                section.PalId = easement.id;
                section.start = offset.Item1 * route.length_mi;
                section.stop = offset.Item2 * route.length_mi;
                yield return section;
            }
        }

        public void ExportPubliclyAccessibleLandSections()
        {
            List<stream> streams = new List<stream>();
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                streams = minnesota.streams.ToList();

                Console.WriteLine("Clearing old entries");
                troutDashContext.PalSection.RemoveRange(
                    troutDashContext.PalSection.Where(i => i.Stream.state1.short_name == "MN"));
                troutDashContext.SaveChanges();
                _sequenceRestarter.RestartSequence("publicly_accessible_land_section_id_seq");
            }

            streams.AsParallel()
                .ForAll(route =>
                {
                    try
                    {
                        var kittleNumber = route.source_id;
                        Console.WriteLine("Getting PAL for " + route.name);
                        Parallel.Invoke(
                            () =>
                            {
                                using (var troutDashContext2 = new TroutDashPrototypeContext())
                                {
                                    var type = "Easement";
                                    var easement = troutDashContext2.PalTypes.First(i => i.type == type);
                                    var easements =
                                        SaveEasements(easement, route, type,
                                            "mndnr_fisheries_acquisition").ToList();
                                    troutDashContext2.PalSection.AddRange(easements);
                                    troutDashContext2.SaveChanges();
                                }
                            },
                            () =>
                            {
                                using (var troutDashContext2 = new TroutDashPrototypeContext())
                                {
                                    var type = "State Park";
                                    var easement = troutDashContext2.PalTypes.First(i => i.type == type);
                                    var stateParks =
                                        SaveEasements(easement, route, "State Park",
                                            "dnr_stat_plan_areas_prk").ToList();
                                    troutDashContext2.PalSection.AddRange(stateParks);
                                    troutDashContext2.SaveChanges();
                                }
                            },
                            () =>
                            {
                                using (var troutDashContext2 = new TroutDashPrototypeContext())
                                {
                                    var type = "WMA";
                                    var easement = troutDashContext2.PalTypes.First(i => i.type == type);
                                    var wmas =
                                        SaveEasements(easement, route, "WMA", "dnr_wma_boundaries_pa")
                                            .ToList();
                                    troutDashContext2.PalSection.AddRange(wmas);
                                    troutDashContext2.SaveChanges();
                                }
                            },
                            () =>
                            {
                                using (var troutDashContext2 = new TroutDashPrototypeContext())
                                {
                                    var type = "State Forest";
                                    var easement = troutDashContext2.PalTypes.First(i => i.type == type);
                                    var stateForests =
                                        SaveEasements(easement, route, "State Forest", "state_forest_management_units")
                                            .ToList();
                                    troutDashContext2.PalSection.AddRange(stateForests);
                                    troutDashContext2.SaveChanges();
                                }
                            }
                            );


                    }
                    catch (Exception)
                    {

                        throw;
                    }
                });

        }


        public void ExportCountyToStreamRelations()
        {
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                Console.WriteLine("Caching minnesota counties and streams...");
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                var mnCounties = minnesota.counties.ToList();
                var mnStreams = minnesota.streams.ToList();
                Console.WriteLine("Finding all streams in each county");
                foreach (var mnCounty in mnCounties)
                {
                    Console.WriteLine(mnCounty.name + " county...");
                    foreach (var mnStream in mnStreams)
                    {
                        var isMatch = mnCounty.OriginalGeometry.Intersects(mnStream.OriginalGeometry);
                        if (isMatch)
                        {
                            Console.WriteLine("   has " + mnStream.name);
                            mnCounty.stream.Add(mnStream);
                            mnCounty.stream_count++;
                        }
                    }

                    troutDashContext.SaveChanges();
                }
                
            }
        }

        private IEnumerable<trout_stream_section> CreateSection(trout_streams_minnesota asdf, StreamRoute route)
        {
            var routeMultilineString = route.OriginalGeometry as IMultiLineString;
            var troutStreamSection = (asdf.OriginalGeometry as IMultiLineString);
            var desiredTroutStreamSection = (asdf.Geometry_4326 as IMultiLineString);
            var TroutStreamSection4236 = (asdf.Geometry_4326 as IMultiLineString);
            var numberOfGeometries = troutStreamSection.Geometries.Count();
            for (var i = 0; i < numberOfGeometries; i++)
            {
                var s = troutStreamSection.Geometries[i] as ILineString;
                var desiredGeometry = desiredTroutStreamSection.Geometries[i] as ILineString;
                desiredGeometry.SRID = 44326;
                var asdf4236 = TroutStreamSection4236.Geometries[i];

                var trout_section = new trout_stream_section();

                var centroid = asdf4236.Centroid;
                trout_section.centroid_latitude = Convert.ToDecimal(centroid.X);
                trout_section.centroid_longitude = Convert.ToDecimal(centroid.Y);
                trout_section.length_mi = Convert.ToDecimal(s.Length) / METERS_IN_MILE;
                trout_section.public_length = 0;
                trout_section.section_name = asdf.kittle_nam ?? "Unnamed Stream";
                trout_section.source_id = asdf.kittle_nbr;

                var multilineString = new MultiLineString(new[] { s });

                var t = new LinearReference();
                var wktWriter = new WKTWriter();
                wktWriter.EmitSRID = true;
                wktWriter.HandleSRID = true;
                var desiredMultilineString = new MultiLineString(new[] {desiredGeometry});
                desiredMultilineString.SRID = 4326;
                var text = wktWriter.Write(desiredMultilineString);
//                var superResultLol = 
                var writer = new WKBWriter();
//                writer.HandleSRID = true;
//                writer.EmitSRID = true;

                var binResult = writer.Write(desiredMultilineString);
                var bin2 = desiredMultilineString.ToBinary();

                var stringResult = System.Text.Encoding.UTF7.GetString(binResult);
                trout_section.Geom = asdf.Geom_4326;
                var result = t.GetIntersectionOfLine(routeMultilineString.Geometries.First() as ILineString, s).ToList();

                trout_section.start = (decimal)result[0] / METERS_IN_MILE;
                trout_section.stop = (decimal)result[1] / METERS_IN_MILE;

                yield return trout_section;

            }



        }
    }
}