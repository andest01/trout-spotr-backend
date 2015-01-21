using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        public void ExportLakeSections()
        {
            
        }

        public IEnumerable<lake_section> SaveEasements(stream route, string table)
        {
            var kittleNumber = route.source_id;
            Console.WriteLine("lake offsets... ");
            var lakes = _troutDashContext.lakes.ToList();
            var offsets = _getLinearOffsets.GetStuff(kittleNumber, table).ToArray();
            foreach (var offset in offsets)
            {
                var section = new lake_section();
                section.stream = route;
                section.lake_gid = lakes;
                section.start = offset.Item1 * route.length_mi;
                section.stop = offset.Item2 * route.length_mi;
                yield return section;
            }
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