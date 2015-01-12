using System.ComponentModel.DataAnnotations.Schema;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    [Table("stream_regs", Schema = "public")]
    public class strm_regsln3 : GeometryExtended
    {
        [Column("kittle_no")]
        public string kittle_no { get; set; }

        [Column("kittname")]
        public string kittname { get; set; }

        [Column("trout_flag")]
        public int trout_flag { get; set; }

        [Column("new_reg")]
        public int new_reg { get; set; }

        [Column("xtrout_fla")]
        public string xtrout_fla { get; set; }
    }
}