using System;

namespace TroutDash.EntityFramework.Models
{
    public partial class spatial_ref_sys
    {
        public int srid { get; set; }
        public string auth_name { get; set; }
        public Nullable<int> auth_srid { get; set; }
        public string srtext { get; set; }
        public string proj4text { get; set; }
    }
}
