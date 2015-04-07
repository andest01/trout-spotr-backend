using System.ComponentModel.DataAnnotations.Schema;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    [Table("state_parks", Schema = "public")]
    public partial class dnr_stat_plan_areas_prk : GeometryExtended
    {
        [Column("area_name")]
        public string area_name { get; set; }

        [Column("area_type")]
        public string area_type { get; set; }
    }
}