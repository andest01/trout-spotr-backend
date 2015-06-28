using System.ComponentModel.DataAnnotations.Schema;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    [Table("dnr_stat_plan_areas_prk", Schema = "public")]
    public partial class dnr_stat_plan_areas_prk : GeometryExtended
    {
        [Column("area_name")]
        public string area_name { get; set; }

        [Column("area_type")]
        public string area_type { get; set; }
    }

    [Table("state_forest_management_units", Schema = "public")]
    public partial class state_forest_management_units : GeometryExtended
    {
        [Column("unit_name")]
        public string unit_name { get; set; }

        [Column("unit_type")]
        public string unit_type { get; set; }
    }
}