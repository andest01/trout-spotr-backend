using System.Collections.Generic;

namespace TroutDash.EntityFramework.Models
{
    public partial class publicly_accessible_land_type
    {
        public publicly_accessible_land_type()
        {
//            this.publicly_accessible_land_sections = new List<publicly_accessible_land_section>();
        }

        public int id { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public bool is_federal { get; set; }
        public int state_gid { get; set; }
        public state state { get; set; }
//        public virtual ICollection<publicly_accessible_land_section> publicly_accessible_land_sections { get; set; }
        public virtual ICollection<publicly_accessible_land> publicly_accessible_lands { get; set; }
    }
}
