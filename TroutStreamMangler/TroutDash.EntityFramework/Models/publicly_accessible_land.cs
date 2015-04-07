using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public publicly_accessible_land ()
        {
            streams = new List<stream>();
        }
        public int gid { get; set; }
        public string area_name { get; set; }
        public int pal_Id { get; set; }
        public int state_gid { get; set; }
        public publicly_accessible_land_type palType { get; set; }
        public state state { get; set; }
        public decimal shape_area { get; set; }
        public ICollection<stream> streams { get; set; }
    }
}