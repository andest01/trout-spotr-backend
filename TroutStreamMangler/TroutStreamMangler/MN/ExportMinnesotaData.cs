using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using GeoAPI.IO;
using ManyConsole;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Union;
using Newtonsoft.Json;
using Npgsql;
using TroudDash.GIS;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN.Models;

namespace TroutStreamMangler.MN
{
    public class ExportMinnesotaData : ConsoleCommand, IDatabaseExporter
    {
        public const decimal METERS_IN_MILE = 1609.34M;
        public ExportMinnesotaData()
        {
            IsCommand("ExportMn", "Exports minnesota data into trout dash. must be run after ImportMn.");
            HasRequiredOption("regulationJson=", "the location of the regulation json", j => FileLocation = j);
        }

        public string FileLocation { get; set; }

        public override int Run(string[] remainingArguments)
        {
            ExportRestrictions();
            ExportStreams();
            ExportPubliclyAccessibleLand();
            ExportCountyToStreamRelations();
            ExportStreamToPubliclyAccessibleLandRelations();
            ExportPubliclyAccessibleLandSections();
            
            return 0;
        }

        private void ExportStreamToPubliclyAccessibleLandRelations()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                Console.WriteLine("Caching minnesota counties and streams...");
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                var mnPals = minnesota.publicly_accessible_land.ToList();
                var mnStreams = minnesota.streams.ToList();
                Console.WriteLine("Finding all streams in each county");
                foreach (var mnStream in mnStreams)
                {
                    Console.WriteLine(mnStream.name + " stream...");
                    foreach (var mnPal in mnPals)
                    {
                        var isMatch = mnPal.OriginalGeometry.Intersects(mnStream.OriginalGeometry);
                        if (isMatch)
                        {
                            Console.WriteLine("   has " + mnPal.area_name);
                            mnPal.streams.Add(mnStream);
                        }
                    }

                    troutDashContext.SaveChanges();
                }

            }
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
            var file = File.ReadAllText(FileLocation);
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

                Console.WriteLine("Deleted all streams in Minnesota");

                // gather ye trout streams.
                ILookup<string, trout_streams_minnesota> troutStreamSectionIds =
                    context.trout_streams_minnesota.Where(ts => ts.trout_flag == 1).ToLookup(tss => tss.kittle_nbr);

                var badList = new List<trout_streams_minnesota>();
                foreach (var section in troutStreamSectionIds)
                {
                    try
                    {
                        var id = section.Key;
                        Console.WriteLine("Loading Trout Stream Id: " + id);
                        var route = context.StreamRoute.Single(sr => sr.kittle_nbr == id);

                        stream stream = CreateStream(route, minnesota);
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

                Console.WriteLine("Failures: ");
                badList.ForEach(b => Console.WriteLine(b.kittle_nam + " " + b.kittle_nbr));
                troutDashContext.SaveChanges();
            }
        }

        private static void SaveValue(TroutDashPrototypeContext troutDashContext, stream stream)
        {
            troutDashContext.streams.Add(stream);
            try
            {
                troutDashContext.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var inners = e.EntityValidationErrors.First();
                Console.WriteLine(inners);
            }

            catch (DbUpdateException e)
            {
            }
        }

        private stream CreateStream(StreamRoute route, state minnesota)
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
            stream.length_mi = Convert.ToDecimal(route.OriginalGeometry.Length) / 1609.3440M; // fix later.
            stream.local_name = route.kittle_nam;
            stream.slug = Guid.NewGuid().ToString();
            stream.source_id = route.kittle_nbr;
            stream.source = route.kittle_nbr;
            stream.state1 = minnesota;
            stream.state = minnesota.Name;
            stream.Geom = route.Geom_3857;
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
                var oldPal = minnesota.publicly_accessible_land_types.ToList();
                troutDashContext.publicly_accessible_land_types.RemoveRange(oldPal);
                troutDashContext.SaveChanges();
                Console.WriteLine("Adding all ublicly accessible land in minnesota");
                minnesota.publicly_accessible_land_types = new List<publicly_accessible_land_type>
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
                    }
                };
                troutDashContext.SaveChanges();

                var easementType =
                    troutDashContext.publicly_accessible_land_types.Single(
                        i => i.state.gid == minnesota.gid && i.type == "Easement");

                var accessableEasements = context.mndnr_fisheries_acquisition
                    .Where(i => i.use_reg != mndnr_fisheries_acquisition.NoAccess)
                    .ToList() //i.use_reg_type != use_reg_types.NoAccess)
                    .Select(i => new publicly_accessible_land
                    {
                        state_gid = minnesota.gid,
                        area_name = i.unit_name,
                        publicly_accessible_land_type_id = easementType.id,
                        shape_area = Convert.ToDecimal(i.OriginalGeometry.Area),
                        Geom = i.Geom_3857
                    }).ToList();
                Console.WriteLine("Loading easements");
                troutDashContext.publicly_accessible_lands.AddRange(accessableEasements);

                var stateparkType =
                    troutDashContext.publicly_accessible_land_types.Single(
                        i => i.state.gid == minnesota.gid && i.type == "State Park");

                var parks = context.dnr_stat_plan_areas_prk
                    .ToList()
                    .Select(p => new publicly_accessible_land
                    {
                        state_gid = minnesota.gid,
                        area_name = p.area_name,
                        publicly_accessible_land_type_id = stateparkType.id,
                        shape_area = Convert.ToDecimal(p.OriginalGeometry.Area),
                        Geom = p.Geom_3857
                    });

                Console.WriteLine("Loading state parks");
                troutDashContext.publicly_accessible_lands.AddRange(parks);

                var wmaType = troutDashContext.publicly_accessible_land_types.Single(
                    i => i.state.gid == minnesota.gid && i.type == "WMA");

                var wmas = context.dnr_wildlife_management_area_boundaries_publicly_accessible
                    .ToList()
                    .Select(i => new publicly_accessible_land
                    {
                        state_gid = minnesota.gid,
                        Geom = i.Geom_3857,
                        publicly_accessible_land_type_id = wmaType.id,
                        area_name = i.unitname,
                        shape_area = Convert.ToDecimal(i.OriginalGeometry.Area)
                    });

                Console.WriteLine("Loading wildlife management areas");
                troutDashContext.publicly_accessible_lands.AddRange(wmas);

                Console.WriteLine("Saving publicly accessible land changes");

                troutDashContext.SaveChanges();
            }
        }

        public void ExportLakes()
        {
            
        }

        public IEnumerable<Tuple<decimal, decimal>> GetStuff(string streamId, string geometryTable)
        {
            const string linearReferenceString =
                @"SELECT (St_dump(St_intersection(palbuffer, strouter.geom))).geom                                                                 as thegeom,
       st_linelocatepoint(st_linemerge(strouter.geom), st_startpoint((st_dump(st_intersection(palbuffer, strouter.geom))).geom)) AS start,
       st_linelocatepoint(st_linemerge(strouter.geom), st_endpoint((st_dump(st_intersection(palbuffer, strouter.geom))).geom))   AS stop
FROM   streams_with_measured_kittle_routes strouter, 
       ( 
              SELECT st_buffer(st_union(dumpgeom), 3, 'endcap=flat join=round') AS palbuffer 
              FROM   ( 
                            SELECT pal.geom                        AS dumpgeom 
                            FROM   {1}        AS pal,
                            streams_with_measured_kittle_routes routes
                            WHERE  ST_Intersects(pal.geom, routes.geom) 
                            and routes.kittle_nbr = '{0}') AS DUMP) AS palbuffer
WHERE  strouter.kittle_nbr = '{0}'
";

            NpgsqlConnection conn =
                new NpgsqlConnection(
                    "Server=localhost;Port=5432;User Id=postgres;Password=fakepassword;Database=mn_import;");
            conn.Open();

            var sql = string.Format(linearReferenceString, streamId, geometryTable);


            NpgsqlCommand command = new NpgsqlCommand(sql, conn);


            try
            {
                NpgsqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    var start = Convert.ToDecimal(dr[1]);
                    var stop = Convert.ToDecimal(dr[2]);

                    yield return new Tuple<decimal, decimal>(start, stop);

                }

            }

            finally
            {
                conn.Close();
            }
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

        public IEnumerable<publicly_accessible_land_section> SaveEasements(TroutDashPrototypeContext troutDashContext, stream route, string palType, string table)
        {
            var kittleNumber = route.source_id;
            Console.WriteLine(palType + "... ");
            var easement = troutDashContext.publicly_accessible_land_types.First(i => i.type == palType);
            var offsets = GetStuff(kittleNumber, table).ToArray();
            foreach (var offset in offsets)
            {
                var section = new publicly_accessible_land_section();
                section.Stream = route;
                section.publicly_accessible_land_type_id = easement.id;
                section.start = offset.Item1 * route.length_mi;
                section.stop = offset.Item2 * route.length_mi;
                yield return section;
            }

            

        }

        public void ExportPubliclyAccessibleLandSections()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                var pal = minnesota.publicly_accessible_land.ToList();
                var streams = minnesota.streams.ToList();

                Console.WriteLine("Clearing old entries");
                troutDashContext.publicly_accessible_land_section.RemoveRange(
                    troutDashContext.publicly_accessible_land_section.Where(i => i.Stream.state1.short_name == "MN"));
                troutDashContext.SaveChanges();

                foreach (var route in streams)
                {
                    Console.WriteLine("Getting PAL for " + route.name);
                    var easements = SaveEasements(troutDashContext, route, "Easement", "mndnr_fisheries_acquisition").ToList();
                    var stateParks = SaveEasements(troutDashContext, route, "State Park", "dnr_stat_plan_areas_prk").ToList();
                    var wmas = SaveEasements(troutDashContext, route, "WMA", "dnr_wildlife_management_area_boundaries_publicly_accessible").ToList();

                    troutDashContext.publicly_accessible_land_section.AddRange(easements);
                    troutDashContext.publicly_accessible_land_section.AddRange(stateParks);
                    troutDashContext.publicly_accessible_land_section.AddRange(wmas);
                    troutDashContext.SaveChanges();
                }


//                var t = new LinearReference();
//                // state parks first
//                foreach (var streamRoute in streams)
//                {
//                    var streamLine = (streamRoute.OriginalGeometry as IMultiLineString).Geometries[0] as ILineString;
//                    var pals = pal.Where(p => p.OriginalGeometry.Intersects(streamRoute.OriginalGeometry)).ToList();
//
//                    var sections = pal.Where(p => p.OriginalGeometry.Intersects(streamRoute.OriginalGeometry)).Select(f => new
//                    {
//                        pal = f,
//                        line = f.OriginalGeometry.Intersection(streamRoute.OriginalGeometry),
//                        offsets = t.GetIntersectionOfLine(streamLine, f.OriginalGeometry.Intersection(streamRoute.OriginalGeometry) as ILineString).ToArray()
//                    }).ToList();
//                    Console.WriteLine(sections);
//
//                    foreach (var result in sections)
//                    {
//                        var pubSection = new publicly_accessible_land_section();
//                        pubSection.start = (decimal)(result.offsets[0] / (double) 1609.3440M);
//                        pubSection.start = (decimal)(result.offsets[1] / (double)1609.3440M);
//                        pubSection.publicly_accessible_land_type2 = result.pal.type;
//                        pubSection.Stream = streamRoute;
//                        troutDashContext.publicly_accessible_land_section.Add(pubSection);
//                        troutDashContext.SaveChanges();
//                    }
                    
                    

//                    var intersectingLines = sections.Select(j => j.OriginalGeometry.Intersection(streamRoute.OriginalGeometry)).ToList();
//                    foreach (publicly_accessible_land section in sections)
//                    {
//                        var streamLine = (streamRoute.OriginalGeometry as IMultiLineString);
//                        var palMultiPolygon = section.OriginalGeometry as IMultiPolygon;
//                        var linearIntersections = t.GetIntersections(streamLine, palMultiPolygon).ToList();
//
////                        var intersectionGeometries =
////                            sections.Select(s => s.OriginalGeometry.Buffer(5).Intersection(streamRoute.OriginalGeometry))
////                                .Cast<ILineString>();
////                        foreach (var part in intersectionGeometries)
////                        {
////                            var streamLine = (streamRoute.OriginalGeometry as IMultiLineString).Geometries[0] as ILineString;
////                            var linearIntersections = t.GetIntersections(streamLine, section.OriginalGeometry);
////                        }
//                    }
//                }

                

            }
        }

        public void ExportCountyToStreamRelations()
        {
            using (var context = new MinnesotaShapeDataContext())
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
            var desiredTroutStreamSection = (asdf.Geometry_3857 as IMultiLineString);
            var TroutStreamSection4236 = (asdf.Geometry_4326 as IMultiLineString);
            var numberOfGeometries = troutStreamSection.Geometries.Count();
            for (var i = 0; i < numberOfGeometries; i++)
            {
                var s = troutStreamSection.Geometries[i] as ILineString;
                var desiredGeometry = desiredTroutStreamSection.Geometries[i] as ILineString;
                desiredGeometry.SRID = 3857;
                var asdf4236 = TroutStreamSection4236.Geometries[i];

                var trout_section = new trout_stream_section();

                var centroid = asdf4236.Centroid;
                trout_section.centroid_latitude = Convert.ToDecimal(centroid.X);
                trout_section.centroid_longitude = Convert.ToDecimal(centroid.Y);
                trout_section.length_mi = Convert.ToDecimal(s.Length) / 1609.3440M;
                trout_section.public_length = 0;
                trout_section.section_name = asdf.kittle_nam ?? "Unnamed Stream";
                trout_section.source_id = asdf.kittle_nbr;

                var multilineString = new MultiLineString(new[] { s });

                var t = new LinearReference();
                var wktWriter = new WKTWriter();
                wktWriter.EmitSRID = true;
                wktWriter.HandleSRID = true;
                var desiredMultilineString = new MultiLineString(new[] {desiredGeometry});
                desiredMultilineString.SRID = 3857;
                var text = wktWriter.Write(desiredMultilineString);
//                var superResultLol = 
                var writer = new WKBWriter();
//                writer.HandleSRID = true;
//                writer.EmitSRID = true;

                var binResult = writer.Write(desiredMultilineString);
                var bin2 = desiredMultilineString.ToBinary();

                var stringResult = System.Text.Encoding.UTF7.GetString(binResult);

//                var text = desiredGeometry.AsText();
//                var _writer = new WKBWriter();
                
//                var bin = multilineString.AsBinary();

//                trout_section.Geom = _writer.(desiredGeometry);//asdf.Geom_3857;
                trout_section.Geom = asdf.Geom_3857;
                var result = t.GetIntersectionOfLine(routeMultilineString.Geometries.First() as ILineString, s).ToList();

                trout_section.start = (decimal)result[0] / 1609.3440M;
                trout_section.stop = (decimal)result[1] / 1609.3440M;

                yield return trout_section;

            }



        }
    }
}