using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{name}, {local_name}")]
    public partial class trout_stream_section : GeometryBase, ISection
    {
        public trout_stream_section()
        {
        }

        public int gid { get; set; }
        public string section_name { get; set; }
        public decimal length_mi { get; set; }
        public decimal public_length { get; set; }
        public decimal centroid_latitude { get; set; }
        public decimal centroid_longitude { get; set; }
        public string source_id { get; set; }
        public decimal start { get; set; }
        public decimal stop { get; set; }
        public virtual stream stream { get; set; } 
        private static readonly WKBReader _reader = new WKBReader();

        public int stream_gid { get; set; }
    }
}
