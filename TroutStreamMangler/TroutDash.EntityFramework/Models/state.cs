using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    public partial class state : GeometryBase
    {
        public state()
        {
            this.counties = new List<county>();
            this.restrictions = new List<restriction>();
            this.streams = new List<stream>();
            this.publicly_accessible_land = new List<publicly_accessible_land>();
            this.publicly_accessible_land_types = new List<publicly_accessible_land_type>();
        }

        public int gid { get; set; }
        public string statefp { get; set; }
        public string short_name { get; set; }
        public string Name { get; set; }
        public virtual ICollection<county> counties { get; set; }
        public virtual ICollection<restriction> restrictions { get; set; }
        public virtual ICollection<stream> streams { get; set; }
        public virtual ICollection<publicly_accessible_land> publicly_accessible_land { get; set; }
        public virtual ICollection<publicly_accessible_land_type> publicly_accessible_land_types { get; set; }
        
    }
}
