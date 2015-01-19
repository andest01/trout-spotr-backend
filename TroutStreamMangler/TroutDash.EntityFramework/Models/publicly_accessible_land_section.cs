using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    public partial class publicly_accessible_land_section : ISection
    {
        public int id { get; set; }
        public decimal start { get; set; }
        public decimal stop { get; set; }
        public int publicly_accessible_land_type_id { get; set; }
        public int stream_gid { get; set; }
        public virtual stream Stream { get; set; }
//        public virtual Pal_type publicly_accessible_land_type { get; set; }
    }
}
