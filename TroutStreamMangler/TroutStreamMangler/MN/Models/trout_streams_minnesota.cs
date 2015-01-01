using System.ComponentModel.DataAnnotations.Schema;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    [Table("trout_streams_minnesota", Schema = "public")]
    public class trout_streams_minnesota : GeometryExtended
    {
        [Column("kittle_nbr")]
        public string kittle_nbr { get; set; }

        [Column("kittle_nam")]
        public string kittle_nam { get; set; }

        [Column("route_mi")]
        public decimal route_mi { get; set; }

        [Column("from_meas")]
        public decimal from_meas { get; set; }

        [Column("to_meas")]
        public decimal to_meas { get; set; }

        [Column("trout_flag")]
        public int trout_flag { get; set; }

        [Column("geom_2d")]
        public string Geom2 { get; set; }

        public IGeometry Geometry2
        {
            get
            {
                var bytes = WKBReader.HexToBytes(Geom2);
                var geom = _reader.Read(bytes);
                return geom;
            }
        }
    }
}