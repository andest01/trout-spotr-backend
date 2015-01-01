using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    [DebuggerDisplay("{name}")]
    public partial class county : GeometryBase
    {
        public county()
        {
            this.stream = new List<stream>();
        }

        public int gid { get; set; }
        public string statefp { get; set; }
        public string countyfp { get; set; }
        public string name { get; set; }
        public string lsad { get; set; }
        public int state_gid { get; set; }
        public virtual state state { get; set; }
        public virtual ICollection<stream> stream { get; set; }
//        private static readonly WKBReader _reader = new WKBReader();
//        [Column("geom")]
//        public string Geom { get; set; }
//        private Lazy<IGeometry> _geom;
//        public virtual GeoAPI.Geometries.IGeometry Geometry
//        {
//            get
//            {
//                var bytes = WKBReader.HexToBytes(Geom);
//                var geom = _reader.Read(bytes);
//                return geom;
//            }
//        }
    }
}
