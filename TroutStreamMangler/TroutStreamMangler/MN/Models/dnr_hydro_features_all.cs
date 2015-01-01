using System.ComponentModel.DataAnnotations.Schema;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    [Table("dnr_hydro_features_all", Schema = "public")]
    public class dnr_hydro_features_all : GeometryExtended
    {
        [Column("gid")]
        public int gid { get; set; }

        [Column("dnr_hydro_")]
        public int dnr_hydro_ { get; set; }

        [Column("pw_pasin_n")]
        public string pw_pasin_n { get; set; }
    }
}