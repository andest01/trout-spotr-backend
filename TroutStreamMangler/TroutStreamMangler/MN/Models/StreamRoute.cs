using System.ComponentModel.DataAnnotations.Schema;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    [Table("streams_with_measured_kittle_routes", Schema = "public")]
    public partial class StreamRoute : GeometryExtended
    {
        [Column("kittle_nbr")]
        public string kittle_nbr { get; set; }

        [Column("kittle_nam")]
        public string kittle_nam { get; set; }

        [Column("from_meas")]
        public decimal from_meas { get; set; }

        [Column("to_meas")]
        public decimal to_meas { get; set; }

        [Column("route_mi")]
        public decimal route_mi { get; set; }

        [Column("length_mi")]
        public decimal length_mi { get; set; }

        [Column("shape_leng")]
        public decimal shape_leng { get; set; }
    }
}