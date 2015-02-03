using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroutDash.Export
{
    public interface IJsonExporter
    {
        IEnumerable<StreamDetails> GetStreamDetails();
    }

    public class StreamDetails
    {
        public StreamDetails()
        {
            Pal = new PalCollection();
            TroutStreams = new TroutStreamCollection();
            Restrictions = new List<RestrictionCollection>();
            Lakes = new LakeCollection();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public decimal LengthMiles { get; set; }
        public bool HasBrookTrout { get; set; }
        public bool HasBrownTrout { get; set; }
        public bool HasRainbowTrout { get; set; }
        public bool HasStockedBrookTrout { get; set; }
        public bool HasStockedBrownTrout { get; set; }
        public bool HasStockedRainbowTrout { get; set; }
        public PalCollection Pal { get; set; }
        public TroutStreamCollection TroutStreams { get; set; }
        public IEnumerable<RestrictionCollection> Restrictions { get; set; }
        public LakeCollection Lakes { get; set; }

        private static decimal GetLength(IEnumerable<ISection> sections)
        {
            return sections == null ? 0 : sections.Sum(s => s.Start + s.Stop);
        }

        public decimal LakesLength
        {
            get { return GetLength(Lakes.Sections); }
        }

        public decimal PalsLength
        {
            get { return GetLength(Pal.Sections); }
        }

        public decimal TroutStreamsLength
        {
            get { return GetLength(TroutStreams.Sections); }
        }

        public decimal RestrictionsLength
        {
            get { return GetLength(Restrictions.SelectMany(i => i.Sections)); }
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
