using System.Collections.Generic;
using System.Diagnostics;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{name}")]
    public partial class road : GeometryBase
    {
        public road()
        {
//            this.stream = new List<stream>();
            this.stream = new List<stream>();
            this.road_crossings = new List<stream_access_point>();
        }

        public int gid { get; set; }
        public string name { get; set; }
        public string local_name { get; set; }
        public string source { get; set; }
        public int road_type_id { get; set; }
        public int state_gid { get; set; }
        public virtual road_type road_type { get; set; }
        public virtual state state { get; set; }
        public virtual ICollection<stream> stream { get; set; }
        public virtual ICollection<stream_access_point> road_crossings { get; set; } 

    }

    [DebuggerDisplay("{name}")]
    public partial class road_crossing : GeometryBase
    {
        public road_crossing()
        {
            //            this.stream = new List<stream>();
        }

        public int gid { get; set; }
        public string street_name { get; set; }
        public bool is_over_publicly_accessible_land { get; set; }
        public int road_gid { get; set; }
        public int stream_gid { get; set; }
        public virtual road road { get; set; }
        public virtual stream stream { get; set; }
    }
}