using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroutDash.Export
{
    public class StreamSpeciesCsv
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Counties { get; set; }
        public string AltName { get; set; }
        public bool? IsBrook { get; set; }
        public bool? IsBrookStocked { get; set; }
        public bool? IsBrown { get; set; }
        public bool? IsBrownStocked { get; set; }
        public bool? IsRainbow { get; set; }
        public bool? IsRainbowStocked { get; set; }
        public string Status { get; set; }
        public string QaPass { get; set; }
    }
    public interface IJsonExporter
    {
        IEnumerable<StreamDetails> GetStreamDetails();
        IEnumerable<RegionDetails> GetRegionDetails();
    }

    public class RegionDetails
    {
        public RegionDetails()
        {
            Counties = new List<CountyDetails>();
        }
        public string RegionName { get; set; }
        public int RegionId { get; set; }
        public List<CountyDetails> Counties { get; set; } 
    }

    public class CountyDetails
    {
        public CountyDetails()
        {
            Streams = new List<StreamDetails>();
        }

        public string CountyName { get; set; }
        public int CountyId { get; set; }
        public string StateName { get; set; }
        public int StateId { get; set; }
        public IEnumerable<StreamDetails> Streams { get; set; } 
    }


    public class MapDetails
    {
        
    }

    public interface IStreamSummary
    {
        int Id { get; set; }
        string Name { get; set; }
        decimal LengthMiles { get; set; }
        decimal CentroidLatitude { get; set; }
        decimal CentroidLongitude { get; set; }
        bool HasBrookTrout { get; set; }
        bool HasBrownTrout { get; set; }
        bool HasRainbowTrout { get; set; }
        bool HasStockedBrookTrout { get; set; }
        bool HasStockedBrownTrout { get; set; }
        bool HasStockedRainbowTrout { get; set; }
        string AlertMessage { get; set; }
        decimal LakesLength { get; }
        decimal PalsLength { get; }
        decimal TroutStreamsLength { get; }
        decimal RestrictionsLength { get; }
    }

    public class StreamSummary : IStreamSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal LengthMiles { get; set; }
        public decimal CentroidLatitude { get; set; }
        public decimal CentroidLongitude { get; set; }
        public bool HasBrookTrout { get; set; }
        public bool HasBrownTrout { get; set; }
        public bool HasRainbowTrout { get; set; }
        public bool HasStockedBrookTrout { get; set; }
        public bool HasStockedBrownTrout { get; set; }
        public bool HasStockedRainbowTrout { get; set; }
        public string AlertMessage { get; set; }
        public virtual decimal LakesLength { get; set; }
        public virtual decimal PalsLength { get; set; }
        public virtual decimal TroutStreamsLength { get; set; }
        public virtual decimal RestrictionsLength { get; set; }
    }

    public class StreamDetails : StreamSummary
    {
        public sealed class CountyModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public StreamDetails()
        {
            Pal = new PalCollection();
            TroutStreams = new TroutStreamCollection();
            Restrictions = new List<RestrictionCollection>();
            Lakes = new LakeCollection();
            LocalNames = new string[0];
            Counties = new CountyModel[0];
        }

        public CountyModel[] Counties { get; set; }

        public string[] LocalNames { get; set; }

        public PalCollection Pal { get; set; }
        public TroutStreamCollection TroutStreams { get; set; }
        public IEnumerable<RestrictionCollection> Restrictions { get; set; }
        public LakeCollection Lakes { get; set; }

        private static decimal GetLength(IEnumerable<ISection> sections)
        {
            return sections == null ? 0 : sections.Sum(s => s.Stop - s.Start);
        }

        public override decimal LakesLength
        {
            get { return Lakes == null ? 0.0M : GetLength(Lakes.Sections); }
        }

        public override decimal PalsLength
        {
            get { return Pal == null ? 0.0M : GetLength(Pal.Sections); }
        }

        public override decimal TroutStreamsLength
        {
            get { return TroutStreams == null ? 0.0M : GetLength(TroutStreams.Sections); }
        }

        public override decimal RestrictionsLength
        {
            get { return Restrictions == null ? 0.0M : GetLength(Restrictions.SelectMany(i => i.Sections)); }
        }
    }

    public class TroutStreamCollection
    {
        public TroutStreamCollection()
        {
            Sections = new TroutStreamSection[0];
        }

        public string Name { get; set; }
        public int ParentStream { get; set; }
        public IEnumerable<TroutStreamSection> Sections { get; set; } 
    }

    public class TroutStreamSection : ISection
    {
        public decimal Start { get; set; }
        public decimal Stop { get; set; }
    }

    public class LakeCollection
    {
        public LakeCollection()
        {
            Sections = new LakeSection[0];
        }

        public IEnumerable<LakeSection> Sections { get; set; } 
    }

    public class LakeSection : ISection
    {
        public decimal Start { get; set; }
        public decimal Stop { get; set; }
        public string Name { get; set; }
        public int LakeId { get; set; }
    }

    public interface ISection
    {
        decimal Start { get; set; }
        decimal Stop { get; set; }
    }

    public class PalCollection
    {
        public PalCollection()
        {
            Sections = new PalSection[0];
        }

        public int Id { get; set; }
        public bool IsFederal { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<PalSection> Sections { get; set; } 
    }

    public class PalSection : ISection
    {
        public decimal Start { get; set; }
        public decimal Stop { get; set; }
        public string Type { get; set; }
    }

    public class RestrictionCollection
    {
        public RestrictionCollection()
        {
            Sections = new RestrictionSection[0];
        }

        public string FullText { get; set; }
        public string ShortText { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool IsAnglingRestriction { get; set; }
        public bool IsHarvestRestriction { get; set; }
        public int Id { get; set; }

        public IEnumerable<RestrictionSection> Sections { get; set; } 
    }

    public class RestrictionSection : ISection
    {
        public RestrictionSection()
        {
            
        }
        public decimal Start { get; set; }
        public decimal Stop { get; set; }

    }
}
