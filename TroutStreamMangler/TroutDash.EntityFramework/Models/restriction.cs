using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TroutDash.EntityFramework.Models
{
    public partial class restriction
    {
        public restriction()
        {
            this.restriction_section = new List<restriction_section>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int id { get; set; }
        public string legal_text { get; set; }
        public string short_text { get; set; }
        public bool isSeasonal { get; set; }
        public Nullable<System.DateTime> start_time { get; set; }
        public Nullable<System.DateTime> end_time { get; set; }
        public bool isAnglingRestriction { get; set; }
        public bool isHarvestRestriciton { get; set; }
        public int state_gid { get; set; }
        public virtual ICollection<restriction_section> restriction_section { get; set; }
        public virtual state state { get; set; }
        public string source_id { get; set; }   
    }
}
