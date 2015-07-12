using System.Diagnostics;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{street_name}")]
    public partial class stream_access_point : GeometryBase
    {
        public stream_access_point()
        {
        }

        public int gid { get; set; }
        public string street_name { get; set; }
        public virtual stream stream { get; set; }
        public virtual road road { get; set; }
        public int stream_gid { get; set; }
        public int road_gid { get; set; }
        public decimal centroid_latitude { get; set; }
        public decimal centroid_longitude { get; set; }
        public decimal linear_offset { get; set; }
        public bool is_accessible { get; set; }
    }
}