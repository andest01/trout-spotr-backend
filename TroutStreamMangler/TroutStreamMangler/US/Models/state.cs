using System.ComponentModel.DataAnnotations.Schema;
using TroutDash.EntityFramework.Models;


namespace TroutStreamMangler.US.Models
{
    [Table("states", Schema = "public")]
    public class state : GeometryExtended
    {
        [Column("gid")]
        public int gid { get; set; }

        [Column("name")]
        public string name { get; set; }

        [Column("statefp")]
        public string statefp { get; set; }
    }
}