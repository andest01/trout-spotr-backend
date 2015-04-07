using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroutStreamMangler.MN.Models
{
    public class RestricitonSection
    {
    }

    public class MinnesotaRestriction
    {
        public int id { get; set; }
        public string text { get; set; }
        public string shortText { get; set; }
        public bool isHarvestRestriction { get; set; }
        public bool isAnglingRestriction { get; set; }
        public bool isTimeRestriction { get; set; }
        public bool isSpeciesRestriction { get; set; }
    }
}