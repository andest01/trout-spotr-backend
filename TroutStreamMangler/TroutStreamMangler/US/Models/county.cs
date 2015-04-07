using System.ComponentModel.DataAnnotations.Schema;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN.Models;

namespace TroutStreamMangler.US.Models
{
    [Table("counties", Schema = "public")]
    public class county : GeometryExtended
    {
        [Column("gid")]
        public int gid { get; set; }

        [Column("name")]
        public string name { get; set; }

        [Column("statefp")]
        public string statefp { get; set; }

        [Column("countyfp")]
        public string countyfp { get; set; }

        [Column("lsad")]
        public string lsad { get; set; }
    }
}