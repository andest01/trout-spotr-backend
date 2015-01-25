using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN.Models;

namespace TroutStreamMangler.MN
{
    public class ExportRegulationSections : IDisposable
    {
        private readonly TroutDashPrototypeContext _troutDashContext;
        private readonly MinnesotaShapeDataContext _minnesotaContext;

        public ExportRegulationSections(TroutDashPrototypeContext troutDashContext,
            MinnesotaShapeDataContext minnesotaContext)
        {
            _troutDashContext = troutDashContext;
            _minnesotaContext = minnesotaContext;
        }

        public void SaveRestrictionSections()
        {
            var minnesota = _troutDashContext.states.Single(s => s.short_name == "MN");
            var pal = minnesota.Pal.ToList();
            var streams = minnesota.streams.ToList();

            Console.WriteLine("Clearing old entries");
            _troutDashContext.restriction_section.RemoveRange(
                _troutDashContext.restriction_section.Where(r => r.restriction.state.short_name == "MN"));
            _troutDashContext.SaveChanges();
        }

        public void Dispose()
        {
            
        }
    }

    public class RegulationsExporter : IDisposable
    {
        private readonly TroutDashPrototypeContext _troutDashContext;
        private readonly MinnesotaShapeDataContext _minnesotaContext;
        private readonly Lazy<state> _getState;
        public LakeExporter(TroutDashPrototypeContext troutDashContext,
            MinnesotaShapeDataContext minnesotaContext)
        {
            _troutDashContext = troutDashContext;
            _minnesotaContext = minnesotaContext;
            _getState = new Lazy<state>(GetState);
        }

        private state GetState()
        {
            return this._troutDashContext.states.First(s => s.short_name == "MN");
        }

        public void Dispose()
        {
            
        }
    }

    public class LakeExporter : IDisposable
    {
        public const decimal METERS_IN_MILE = 1609.34M;
        private readonly TroutDashPrototypeContext _troutDashContext;
        private readonly MinnesotaShapeDataContext _minnesotaContext;
        private readonly GetLinearOffsets _getLinearOffsets;

        public LakeExporter(TroutDashPrototypeContext troutDashContext,
            MinnesotaShapeDataContext minnesotaContext)
        {
            _troutDashContext = troutDashContext;
            _minnesotaContext = minnesotaContext;
            _getLinearOffsets = new GetLinearOffsets();
            _getState = new Lazy<state>(GetState);
        }

        private Lazy<state> _getState;

        public void ExportLakeSections()
        {
            Console.WriteLine("Exporting Lake Sections...");
            var state = _troutDashContext.states.First(s => s.short_name == "MN");
            var pre_existing_sections = state.streams.SelectMany(s => s.lake_sections);
            _troutDashContext.lake_sections.RemoveRange(pre_existing_sections);
            _troutDashContext.SaveChanges();
            var lakes = state.lakes.ToDictionary(i => i.source_id);
            var streams = state.streams;
            foreach (var stream in streams)
            {
                Console.WriteLine(stream.name + "...");
                var sections = ExportLakeSections(state, stream, lakes).ToList();
                _troutDashContext.lake_sections.AddRange(sections);
                _troutDashContext.SaveChanges();
            }
        }

        private state GetState()
        {
            return this._troutDashContext.states.First(s => s.short_name == "MN");
        }

        public IEnumerable<lake_section> ExportLakeSections(state currentState, stream route, Dictionary<string, lake> lakes)
        {
            var gid = route.source_id;
            var streamTableName = "streams_with_measured_kittle_routes";
            var streamIdColumn = "gid";
            var streamNameColumn = "kittle_nam";

            var geometryNameColumn = "pw_basin_n";
            var geometryIdColumn = "dnr_hydro_";
            var geometryTableName = "dnr_hydro_features_all";

            var results = GetLinearOffsets.ExecuteLinearReference(streamTableName, streamNameColumn, streamIdColumn, gid.ToString(),
                geometryTableName, geometryIdColumn, geometryNameColumn);

            // get lakes.
            

            foreach (var result in results)
            {
                var lakeId = result.GeometryId;
                var start = result.StartPoint;
                var stop = result.EndPoint;
                
                var section = new lake_section();
                section.lake = lakes[lakeId.ToString()];
                section.stream = route;
                section.start = Convert.ToDecimal(start) * route.length_mi;
                section.stop = Convert.ToDecimal(stop) * route.length_mi;
                Console.WriteLine("  " + result.GeometryName ?? "unkown name");
                yield return section;
            }
//            var kittleNumber = route.source_id;
//            Console.WriteLine("lake offsets... ");
//            var lakes = _troutDashContext.lakes.ToList();
//            var offsets = _getLinearOffsets.GetStuff(kittleNumber, table).ToArray();
//            foreach (var offset in offsets)
//            {
//                var section = new lake_section();
//                section.stream = route;
//                section.lake_gid = lakes;
//                section.start = offset.Item1 * route.length_mi;
//                section.stop = offset.Item2 * route.length_mi;
//                yield return section;
//            }
        }

        public void ExportLakes()
        {
            var minnesota = _troutDashContext.states.Single(s => s.short_name == "MN");
            _troutDashContext.lakes.RemoveRange(minnesota.lakes);
            _troutDashContext.SaveChanges();

            

            var lakes = _minnesotaContext.Lakes.ToList().Select(i => new lake
            {
                Geom = i.Geom_3857,
                name = i.pw_pasin_n ?? "Unnamed",
                source_id = i.dnr_hydro_.ToString(),
                state = minnesota,
                state_gid = minnesota.gid,
                is_trout_lake = false
            });

            _troutDashContext.lakes.AddRange(lakes);
            _troutDashContext.SaveChanges();
        }

        public void Dispose()
        {

        }
    }
}