using System;
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