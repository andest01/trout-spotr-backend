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
using NetTopologySuite.Operation.Union;
using Newtonsoft.Json;
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
//            ExportRestrictions();
//            ExportStreams();
//            ExportPubliclyAccessibleLand();
            ExportPubliclyAccessibleLandSections();
//            ExportCountyToStreamRelations();
            return 0;
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

                        var stream = CreateStream(route, minnesota, section);
                        Console.WriteLine("Saving trout stream: " + stream.name);
                        troutDashContext.streams.Add(stream);
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

        private stream CreateStream(StreamRoute route, state minnesota,
            IEnumerable<trout_streams_minnesota> trout_stream_sectionIds)
        {
            var name = route.kittle_nam ?? "Unnamed Stream";
            Console.WriteLine("Adding stream " + name + " | " + route.kittle_nbr);
            var stream = new stream();
            var centroid = route.OriginalGeometry.Centroid;
            stream.centroid_latitude = Convert.ToDecimal(centroid.X);
            stream.centroid_longitude = Convert.ToDecimal(centroid.Y);
            stream.has_brook_trout = true;
            stream.has_brown_trout = true;
            stream.has_rainbow_trout = true;
            stream.is_brook_trout_stocked = true;
            stream.is_brown_trout_stocked = true;
            stream.is_rainbow_trout_stocked = true;
            stream.length_mi = Convert.ToDecimal(route.OriginalGeometry.Length); // fix later.
            stream.local_name = route.kittle_nam;
            stream.slug = Guid.NewGuid().ToString();
            stream.source_id = route.kittle_nbr;
            stream.source = route.kittle_nbr;
            stream.state1 = minnesota;
            stream.state = minnesota.Name;
            stream.Geom = route.Geom_3857;
            stream.name = route.kittle_nam ?? "Unnamed Stream";
            stream.status_mes = "Flood";
            stream.trout_stream_sections = trout_stream_sectionIds.Select(CreateSection).ToList();
            return stream;
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
                minnesota.publicly_accessible_land_types = new List<Pal_type>
                {
                    new Pal_type
                    {
                        state = minnesota,
                        description = "Wildlife Management Area",
                        is_federal = false,
                        type = "WMA"
                    },
                    new Pal_type
                    {
                        state = minnesota,
                        description = "State Parks",
                        is_federal = false,
                        type = "State Park",
                    },
                    new Pal_type
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

        public void ExportPubliclyAccessibleLandSections()
        {
            using (var context = new MinnesotaShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                var minnesota = troutDashContext.states.Single(s => s.short_name == "MN");
                // state parks first
                var idOfStateParkType = minnesota.publicly_accessible_land_types.Single(i => i.type == "State Park").id;

                var beaverCreekPark = minnesota.publicly_accessible_land.Single(p => p.area_name == "Beaver Creek Valley");
                var beaverCreek = minnesota.streams.Single(s => s.source_id == "M-009-010-003-008");
//                var g = beaverCreek.DbGeometry2;
                var originalBeaverCreek = context.StreamRoute.Single(i => i.kittle_nbr == "M-009-010-003-008");
                var shpLength = originalBeaverCreek.shape_leng;
                var length_mi = originalBeaverCreek.length_mi;
                var length = originalBeaverCreek.OriginalGeometry.Length;
                var newLength = originalBeaverCreek.Geometry_4326.Length;
                var recordedLength = originalBeaverCreek.route_mi;

                var pointsOfIntersection = ((IMultiPolygon) beaverCreekPark.OriginalGeometry).Geometries;

//                var stateParks =
//                    minnesota.publicly_accessible_land.Where(
//                        pal => pal.publicly_accessible_land_type_id == idOfStateParkType);
//
//                var disolvedStateParks = CascadedPolygonUnion.Union(stateParks.Select(sp => sp.Geometry).ToList());

                // Use ST_Intersection(ST_ExteriorRing(pal.geom), river.geom) to get the POINTs at which the land crosses the river.
//                var parkExteriorRing = disolvedStateParks.
                // Use ST_LineMerge to build all the river lines up into a SINGLE river line
                // If the river direction is backwards us ST_Reverse to make sure it goes in an upstream direction
                // Use ST_Line_Locate_Point to calculate the proportion up the river each land crossing point is
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

        private trout_stream_section CreateSection(trout_streams_minnesota section)
        {
            var trout_section = new trout_stream_section();
            var centroid = section.OriginalGeometry.Centroid;
            trout_section.centroid_latitude = Convert.ToDecimal(centroid.X);
            trout_section.centroid_longitude = Convert.ToDecimal(centroid.Y);
            trout_section.length_mi = Convert.ToDecimal(section.OriginalGeometry.Length);
            trout_section.public_length = Convert.ToDecimal(section.OriginalGeometry.Length);
            trout_section.section_name = section.kittle_nam ?? "Unnamed Stream";
            trout_section.source_id = section.kittle_nbr;
            trout_section.start = 0;
            trout_section.Geom = section.Geom_3857;
            trout_section.stop = 1;

            return trout_section;
        }
    }
}