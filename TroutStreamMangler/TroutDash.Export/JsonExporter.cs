using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using TroutDash.EntityFramework.Models;

namespace TroutDash.Export
{
    public class JsonExporter : IJsonExporter
    {
        Random rand = new Random();
        private string[] Messages = new string[]
        {
            "Flood",
            "Warm",
            "Closed",
            "Dry",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            ""

        };
        private readonly TroutDashPrototypeContext _context;

        public JsonExporter(TroutDashPrototypeContext context)
        {
            _context = context;
        }

        public IEnumerable<RegionDetails> GetRegionDetails()
        {
            foreach (var region in _context.regions.Include(x => x.counties).ToList())
            {
                var r = new RegionDetails();
                r.RegionName = region.name;
                r.RegionId = region.gid;
                
                foreach (var county in region.counties.Where(c => c.stream.Any()).ToList())
                {
                    var c = new CountyDetails();
                    c.CountyName = county.name;
                    c.CountyId = county.gid;
                    c.StateId = county.state.gid;
                    c.StateName = county.state.Name;

                    var streams = RemoveSmallStreams(county.stream).ToList();
                    var streamDetails = GetStreamDetails(streams).OrderBy(i => i.Name).ToList();
                    c.Streams = streamDetails;

                    r.Counties.Add(c);
                }


                yield return r;
            }
        }

        public IEnumerable<StreamDetails> GetStreamDetails()
        {
            var allStreams = _context.streams.Where(s => s.state == "Minnesota").ToList();
            var streams = RemoveSmallStreams(allStreams);
            foreach (var streamDetails in GetStreamDetails(streams))
            {
                yield return streamDetails;
            }
        }

        private static List<stream> RemoveSmallStreams(IEnumerable<stream> allStreams)
        {
            var streams =
                allStreams.Where(s => (s.name.IndexOf("Unnamed", StringComparison.Ordinal) >= 0 && s.length_mi < 0.2M) == false)
                    .Where(s => (s.length_mi < 0.3M) == false).ToList();
            return streams;
        }

        private IEnumerable<StreamDetails> GetStreamDetails(List<stream> streams)
        {
            foreach (var stream in streams)
            {
                var details = CreateStreamDetails(stream);

                if (details.TroutStreamsLength < 0.2M)
                {
                    continue;
                }

                yield return details;
            }
        }

        private StreamDetails CreateStreamDetails(stream s)
        {
            var d = new StreamDetails();
            d.Id = s.gid;
            d.Name = s.name;
            d.LengthMiles = decimal.Round(s.length_mi, 2, MidpointRounding.AwayFromZero); 
            d.HasBrookTrout = s.has_brook_trout;
            d.HasBrownTrout = s.has_brown_trout;
            d.HasRainbowTrout = s.has_rainbow_trout;
            d.HasStockedBrookTrout = s.is_brook_trout_stocked;
            d.HasStockedRainbowTrout = s.is_rainbow_trout_stocked;
            d.HasStockedBrownTrout = s.is_brown_trout_stocked;
            d.CentroidLatitude = decimal.Round(s.centroid_latitude, 5, MidpointRounding.AwayFromZero); // s.centroid_latitude;
            d.CentroidLongitude = decimal.Round(s.centroid_longitude, 5, MidpointRounding.AwayFromZero); //s.centroid_longitude;
            
            var roll = rand.Next(0, Messages.Length - 1);
            d.AlertMessage = Messages[roll];
            d.Counties = s.counties.Select(i => new StreamDetails.CountyModel
            {
                Id = i.gid,
                Name = i.name
            }).ToArray();

            var localNames = s.trout_stream_sections.Select(i => i.section_name).Where(i => i != d.Name).ToList();
            if (String.IsNullOrWhiteSpace(s.local_name) == false)
            {
                localNames.Add(s.local_name);
            }
            d.LocalNames = localNames.ToArray();

            if (d.LocalNames.Any())
            {
                var priorAltNames = d.LocalNames.ToArray();
                var priorCount = priorAltNames.Count();
                // remove substring duplicates. West Beaver Creek vs Beaver Creek
                // we may want to change this.
                var x = d.LocalNames.Distinct().Where(name => d.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0
                    || name.IndexOf(d.Name, StringComparison.OrdinalIgnoreCase) < 0).ToArray();

                // if primary name is "unnamed", then remove all alternative names with "unnamed".
                if (d.Name.IndexOf("Unnamed", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    x = x.Where(i => i.IndexOf("Unnamed", StringComparison.OrdinalIgnoreCase) < 0).ToArray();
                }

                d.LocalNames = x;

                var currentCount = d.LocalNames.Count();
                if (currentCount != priorCount)
                {
                    
                }
            }

            var pal = s.publicly_accessible_land_section2.ToList();
            var palCollection = new PalCollection();
            palCollection.Sections = pal.Select(pa => new PalSection()
            {
                Start = decimal.Round(pa.start, 3, MidpointRounding.AwayFromZero),
                Stop = decimal.Round(pa.stop, 3, MidpointRounding.AwayFromZero),
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
                Start = decimal.Round(l.start, 3, MidpointRounding.AwayFromZero), //l.start,
                Stop = decimal.Round(l.stop, 3, MidpointRounding.AwayFromZero),
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
                Start = decimal.Round(t.start, 3, MidpointRounding.AwayFromZero), //t.start,
                Stop = decimal.Round(t.stop, 3, MidpointRounding.AwayFromZero), //t.stop
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

                var x  = restrictionSection.Select(i => new RestrictionSection
                {
                    Start = decimal.Round((decimal)i.start, 3, MidpointRounding.AwayFromZero),
                    Stop = decimal.Round((decimal)i.stop, 3, MidpointRounding.AwayFromZero)
                }).ToList();

                var numberOfSections = x.Count;

                var shortSections = x.Where(i =>
                {
//                    var isStartOrEnd = i.Start == 0.0M || NearlyEquals(i.Stop, s.length_mi, 0.001M);
//                    if (isStartOrEnd == false)
//                    {
//                        return true;
//                    }

                    var length = Math.Abs(i.Stop - i.Start);
                    var isTooShort = length < 0.02M;
                    var isTooFewSections = numberOfSections <= 1;

                    if (isTooShort)
                    {
                        if (isTooFewSections)
                        {
                            return false;
                        }
                    }

                    return true;
                }).ToList();

                if (shortSections.Any() == false)
                {
                    continue;
                }

                restrictionCollection.Sections = shortSections;

                yield return restrictionCollection;
            }

            
        }

        private static bool NearlyEquals(decimal value1, decimal value2, decimal unimportantDifference = 0.0001M)
        {
            if (value1 != value2)
            {
                return Math.Abs(value1 - value2) < unimportantDifference;
            }

            return true;
        }
    }
}