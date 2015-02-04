using System;
using System.Collections.Generic;
using System.Linq;
using TroutDash.EntityFramework.Models;

namespace TroutDash.Export
{
    public class JsonExporter : IJsonExporter
    {
        private readonly TroutDashPrototypeContext _context;

        public JsonExporter(TroutDashPrototypeContext context)
        {
            _context = context;
        }

        public IEnumerable<StreamDetails> GetStreamDetails()
        {
            foreach (var stream in _context.streams.Where(s => s.state == "Minnesota").ToList())
            {
                yield return CreateStreamDetails(stream);
            }
        }

        private StreamDetails CreateStreamDetails(stream s)
        {
            var d = new StreamDetails();
            d.Id = s.gid;
            d.Name = s.name;
            d.LengthMiles = s.length_mi;
            d.HasBrookTrout = s.has_brook_trout;
            d.HasBrownTrout = s.has_brown_trout;
            d.HasRainbowTrout = s.has_rainbow_trout;
            d.HasStockedBrookTrout = s.is_brook_trout_stocked;
            d.HasStockedRainbowTrout = s.is_rainbow_trout_stocked;
            d.HasStockedBrownTrout = s.is_brown_trout_stocked;

            var pal = s.publicly_accessible_land_section2.ToList();
            var palCollection = new PalCollection();
            palCollection.Sections = pal.Select(pa => new PalSection()
            {
                Start = pa.start,
                Stop = pa.stop,
                Type = String.Empty
            }).ToList();

            palCollection.Id = 0;
            palCollection.IsFederal = false;
            palCollection.Name = String.Empty;
            palCollection.Type = String.Empty;

            d.Pal = palCollection;

            var lakeSections = s.lake_sections.ToList();
            var lakeCollection = new LakeCollection();
            lakeCollection.Sections = lakeSections.Select(l => new LakeSection
            {
                Start = l.start,
                Stop = l.stop,
                Name = _context.lakes.Single(la => la.gid == l.lake_gid).name,
                LakeId = l.lake_gid
            }).ToList();

            d.Lakes = lakeCollection;

            var troutStreamSections = s.trout_stream_sections.ToList();
            var troutStream = new TroutStreamCollection();
            troutStream.Name = s.name;
            troutStream.ParentStream = s.gid;
            troutStream.Sections = troutStreamSections.Select(t => new TroutStreamSection
            {
                Start = t.start,
                Stop = t.stop
            }).ToList();
            d.TroutStreams = troutStream;

            d.Restrictions = GetRestrictionSections(s).ToList();

            return d;

        }

        private IEnumerable<RestrictionCollection> GetRestrictionSections(stream s)
        {
            var restrictionSections = s.restriction_section.GroupBy(r => r.restriction).ToList();
            
            foreach (var restrictionSection in restrictionSections)
            {
                var restrictionCollection = new RestrictionCollection();
                restriction restrictionType = restrictionSection.Key;
                restrictionCollection.FullText = restrictionType.legal_text;
                restrictionCollection.ShortText = restrictionType.short_text;
                restrictionCollection.Id = restrictionType.id;

                restrictionCollection.Sections = restrictionSection.Select(i => new RestrictionSection
                {
                    Start = (decimal) i.start,
                    Stop = (decimal) i.stop
                }).ToList();

                yield return restrictionCollection;
            }

            
        }
    }
}