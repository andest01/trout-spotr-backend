using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{name}")]
    public partial class publicly_accessible_land : GeometryBase
    {
        public int gid { get; set; }
        public string area_name { get; set; }
        public int publicly_accessible_land_type_id { get; set; }
        public int state_gid { get; set; }
        public Pal_type type { get; set; }
        public state state { get; set; }
        public decimal shape_area { get; set; }
        public ICollection<stream> streams { get; set; }
    }
}