using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{name}, {local_name}")]
    public partial class stream : GeometryBase
    {
        public stream()
        {
//            this.publicly_accessible_land_section2 = new List<publicly_accessible_land_section>();
            this.restriction_section = new List<restriction_section>();
            this.counties = new List<county>();
            this.trout_stream_sections = new List<trout_stream_section>();
            this.lake_sections = new Collection<lake_section>();
        }

        public int gid { get; set; }
        public string name { get; set; }
        public string local_name { get; set; }
        public decimal length_mi { get; set; }
        public decimal centroid_latitude { get; set; }
        public decimal centroid_longitude { get; set; }
        public bool has_brown_trout { get; set; }
        public bool has_brook_trout { get; set; }
        public bool has_rainbow_trout { get; set; }
        public bool is_brown_trout_stocked { get; set; }
        public bool is_brook_trout_stocked { get; set; }
        public bool is_rainbow_trout_stocked { get; set; }
        public string status_mes { get; set; }
        public string state { get; set; }
        public string source { get; set; }
        public int state_gid { get; set; }
        public string source_id { get; set; }
        public string slug { get; set; }
        public virtual ICollection<publicly_accessible_land> publicly_accessible_lands { get; set; } 
        public virtual ICollection<publicly_accessible_land_section>    publicly_accessible_land_section2 { get; set; }
        public virtual ICollection<restriction_section> restriction_section { get; set; }
        public virtual state state1 { get; set; }
        public virtual ICollection<county> counties { get; set; }
        public virtual ICollection<trout_stream_section> trout_stream_sections { get; set; }
        public virtual ICollection<lake_section> lake_sections { get; set; }
    }
}