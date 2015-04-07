using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using TroutDash.DatabaseImporter.Convention;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN.Models;

namespace TroutStreamMangler.MN
{
    public class ExportRegulationSections : IDisposable
    {
        private readonly TroutDashPrototypeContext _troutDashContext;
        private readonly MinnesotaShapeDataContext _minnesotaContext;
        private readonly SequenceRestarter _sequenceRestarter;

        public ExportRegulationSections(TroutDashPrototypeContext troutDashContext,
            MinnesotaShapeDataContext minnesotaContext, IDatabaseConnection dbConnection)
        {
            _troutDashContext = troutDashContext;
            _minnesotaContext = minnesotaContext;
            _sequenceRestarter = new SequenceRestarter(dbConnection);
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
        private readonly TroutDashPrototypeContext _troutDashContext2;
        private readonly MinnesotaShapeDataContext _minnesotaContext2;
        private readonly SequenceRestarter _sequenceRestarter;
        private readonly GetLinearOffsets _getLinearOffsets;
        public RegulationsExporter(TroutDashPrototypeContext troutDashContext,
            MinnesotaShapeDataContext minnesotaContext, SequenceRestarter sequenceRestarter, GetLinearOffsets getLinearOffsets)
        {
            _troutDashContext2 = troutDashContext;
            _minnesotaContext2 = minnesotaContext;
            _sequenceRestarter = sequenceRestarter;
            _getLinearOffsets = getLinearOffsets;
        }

        public void ExportRestrictionSections()
        {
            Console.WriteLine("Exporting Restriction Sections...");
            var state = _troutDashContext2.states.First(s => s.short_name == "MN");
            var preExistingSections = state.streams.SelectMany(s => s.restriction_section);
            _troutDashContext2.restriction_section.RemoveRange(preExistingSections);
            _troutDashContext2.SaveChanges();
            _sequenceRestarter.RestartSequence("restriction_section_id_seq");
            var restrictions = state.restrictions.ToDictionary(i => i.source_id);

            state.streams.ToList()
                .AsParallel()
                .ForAll(stream => Stuff(stream, restrictions));

        }

        private void Stuff(stream s, Dictionary<string, restriction> restrictions)
        {
            try
            {
                using (var troutDashContext = new TroutDashPrototypeContext())
                {
                    Console.WriteLine(s.name + "...");
                    var sections = GetSections(s, restrictions).ToList();
                    troutDashContext.restriction_section.AddRange(sections);
                    troutDashContext.SaveChanges();
                }
            }
            catch (Exception)
            {

            }
        }

        private IEnumerable<restriction_section> GetSections(stream route, Dictionary<string, restriction> restrictions)
        {
            var gid = route.source_id;
            var streamTableName = "streams_with_measured_kittle_routes";
            var streamIdColumn = "gid";
            var streamNameColumn = "kittle_nam";

            var geometryNameColumn = "trout_flag";
            var geometryIdColumn = "new_reg";
            var geometryTableName = "strm_regsln3";

            var results = _getLinearOffsets.ExecuteBufferedLinearReferenceResults(streamTableName, streamNameColumn, streamIdColumn, gid.ToString(),
                geometryTableName, geometryIdColumn, geometryNameColumn);

            foreach (var result in results)
            {
                var restrictionId = result.GeometryId;
                var start = result.StartPoint;
                var stop = result.EndPoint;

                var section = new restriction_section();
                section.restriction_id = restrictions[restrictionId.ToString()].id;
                section.stream_gid = route.gid;
                section.start = Convert.ToDouble(start) * (double)route.length_mi;
                section.stop = Convert.ToDouble(stop) * (double)route.length_mi;
                Console.WriteLine("  " + result.GeometryName ?? "unkown name");
                yield return section;
            }
        }

        public void Stuff(state currentState, stream route, Dictionary<string, lake> lakes)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}