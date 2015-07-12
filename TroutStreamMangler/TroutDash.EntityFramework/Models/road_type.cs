using System.Collections.Generic;
using System.Diagnostics;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{name}")]
    public partial class road_type
    {
        public road_type()
        {
            this.roads = new List<road>();
        }

        public int id { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string source { get; set; }
        public int? state_gid { get; set; }

        public virtual state state { get; set; }
        public virtual ICollection<road> roads { get; set; } 
    }
}