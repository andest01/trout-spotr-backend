using System.ComponentModel.DataAnnotations.Schema;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    [Table("streets_load", Schema = "public")]
    public class streets_load : GeometryExtended
    {
        [Column("street_nam")]
        public string street_nam { get; set; }

        [Column("street_typ")]
        public string street_typ { get; set; }

        [Column("tis_code")]
        public string tis_code { get; set; }

        [Column("route_syst")]
        public string route_syst { get; set; }

        [Column("route_numb")]
        public string route_numb { get; set; }
    }
}