using System;
using System.Collections.Generic;
using System.Linq;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN.Models;

namespace TroutStreamMangler.MN
{
    public class LakeExporter : IDisposable
    {
//        public SequenceRestarter SequenceRestarter { get; set; }
        public const decimal METERS_IN_MILE = 1609.34M;
        private readonly TroutDashPrototypeContext _troutDashContext;
        private readonly MinnesotaShapeDataContext _minnesotaContext;
        private readonly GetLinearOffsets _getLinearOffsets;
        private readonly SequenceRestarter _sequenceRestarter;

        public LakeExporter(TroutDashPrototypeContext troutDashContext,
            MinnesotaShapeDataContext minnesotaContext, SequenceRestarter sequenceRestarter, GetLinearOffsets getLinearOffsets)
        {
            _sequenceRestarter = sequenceRestarter;
            _troutDashContext = troutDashContext;
            _minnesotaContext = minnesotaContext;
            _getLinearOffsets = getLinearOffsets;
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

            state.streams
                .ToList()
                .AsParallel()
                .ForAll(stream =>
                {
                    try
                    {
                        using (var troutDashContext = new TroutDashPrototypeContext())
                        {
                            Console.WriteLine(stream.name + "...");
                            var sections = ExportLakeSections(stream, lakes).ToList();
                            troutDashContext.lake_sections.AddRange(sections);
                            troutDashContext.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                });




                
                
            
        }

        private state GetState()
        {
            return this._troutDashContext.states.First(s => s.short_name == "MN");
        }

        public IEnumerable<lake_section> ExportLakeSections(stream route, Dictionary<string, lake> lakes)
        {
            var gid = route.source_id;
            var streamTableName = "streams_with_measured_kittle_routes";
            var streamIdColumn = "gid";
            var streamNameColumn = "kittle_nam";

            var geometryNameColumn = "pw_basin_n";
            var geometryIdColumn = "dnr_hydro_";
            var geometryTableName = "dnr_hydro_features_all";

            var results = _getLinearOffsets.ExecuteLinearReference(streamTableName, streamNameColumn, streamIdColumn, gid.ToString(),
                geometryTableName, geometryIdColumn, geometryNameColumn);

            // get lakes.
            

            foreach (var result in results)
            {
                var lakeId = result.GeometryId;
                var start = result.StartPoint;
                var stop = result.EndPoint;
                
                var section = new lake_section();
                section.lake_gid = lakes[lakeId.ToString()].gid;
                section.stream_gid = route.gid;
                section.start = Convert.ToDecimal(start) * route.length_mi;
                section.stop = Convert.ToDecimal(stop) * route.length_mi;
                Console.WriteLine("  " + result.GeometryName ?? "unkown name");
                yield return section;
            }
        }

        public void ExportLakes()
        {
            Console.WriteLine("Exporting Minnesota lakes...");
            var minnesota = _troutDashContext.states.Single(s => s.short_name == "MN");
            Console.WriteLine("Deleting Minnesota lakes...");
            _troutDashContext.lakes.RemoveRange(minnesota.lakes);
            _troutDashContext.SaveChanges();
            Console.WriteLine("Deleted Minnesota lakes...");
            _sequenceRestarter.RestartSequence("lake_gid_seq");
            

            var lakes = _minnesotaContext.Lakes.ToList().Select(i => new lake
            {
                Geom = i.Geom_4326,
                name = i.pw_pasin_n ?? "Unnamed",
                source_id = i.dnr_hydro_.ToString(),
                state = minnesota,
                state_gid = minnesota.gid,
                is_trout_lake = false
            });

            Console.WriteLine("Saving Minnesota lakes...");
            _troutDashContext.lakes.AddRange(lakes);
            _troutDashContext.SaveChanges();
            Console.WriteLine("Saved Minnesota lakes.");
        }

        public void Dispose()
        {

        }
    }
}