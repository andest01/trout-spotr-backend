using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    //dnr_wildlife_management_area_boundaries_publicly_accessible
    [Table("dnr_wildlife_management_area_boundaries_publicly_accessible", Schema = "public")]
    public class dnr_wildlife_management_area_boundaries_publicly_accessible : GeometryExtended
    {
        [Key]
        [Column("gid")]
        public int gid { get; set; }

//        [Column("geom")]
//        public string Geom { get; set; }
//
//        private Lazy<IGeometry> _geom;
//        private static readonly WKBReader _reader = new WKBReader();
//
//        public virtual IGeometry Geometry
//        {
//            get
//            {
//                var bytes = WKBReader.HexToBytes(Geom);
//                var geom = _reader.Read(bytes);
//                return geom;
//            }
//        }

        [Column("id")]
        public int id { get; set; }

        // WMA0900901
        [Column("uniqueid")]
        public string uniqueid { get; set; }

        // Whitewater WMA
        [Column("unitname")]
        public string unitname { get; set; }
    }
}