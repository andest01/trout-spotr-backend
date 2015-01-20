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
            var pal = minnesota.publicly_accessible_land.ToList();
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
}