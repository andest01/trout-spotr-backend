using System;
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
        private readonly TroutDashPrototypeContext _troutDashContext;
        private readonly MinnesotaShapeDataContext _minnesotaContext;

        public LakeExporter(TroutDashPrototypeContext troutDashContext,
            MinnesotaShapeDataContext minnesotaContext)
        {
            _troutDashContext = troutDashContext;
            _minnesotaContext = minnesotaContext;
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
                state = minnesota
            });

            _troutDashContext.lakes.AddRange(lakes);
            _troutDashContext.SaveChanges();
        }

        public void Dispose()
        {

        }
    }
}