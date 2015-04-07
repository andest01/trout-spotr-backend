using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{name}")]
    public partial class region : GeometryBase
    {
        public region()
        {
            this.counties = new Collection<county>();
        }

        public string name { get; set; }
        public int gid { get; set; }
        public string long_name { get; set; }
        public virtual ICollection<county> counties { get; set; }
    }
}