using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{name}")]
    public partial class lake : GeometryBase
    {
        public lake()
        {
            this.lake_sections = new Collection<lake_section>();
        }

        public int gid { get; set; }
        public int state_gid { get; set; }
        public string source_id { get; set; }
        public state state { get; set; }
        public string name { get; set; }
        public bool is_trout_lake { get; set; }
        public virtual ICollection<lake_section> lake_sections { get; set; }
    }

    [DebuggerDisplay("{name}")]
    public partial class lake_section : ISection
    {
        public lake_section()
        {

        }

        public int gid { get; set; }
        public decimal start { get; set; }
        public decimal stop { get; set; }
        public lake lake { get; set; }
        public stream stream { get; set; }
        public int lake_gid { get; set; }
        public int stream_gid { get; set; }
    }
}