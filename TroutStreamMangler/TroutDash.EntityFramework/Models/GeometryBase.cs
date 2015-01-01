using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    public abstract class GeometryBase : IGeo
    {
        protected GeometryBase()
        {
            _geom = new Lazy<IGeometry>(() => GetGeometry(() => Geom));
        }

        protected static IGeometry GetGeometry(Func<String> getColumn)
        {
            var bytes = WKBReader.HexToBytes(getColumn());
            var geom = _reader.Read(bytes);
            return geom;
        }

        [Key]
        [Column("gid")]
        public int gid { get; set; }

        [Column("geom")]
        public virtual string Geom { get; set; }

        private Lazy<IGeometry> _geom;
        protected static readonly WKBReader _reader = new WKBReader();


        public virtual IGeometry OriginalGeometry
        {
            get { return _geom.Value; }
        }





        protected static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }

    public abstract class GeometryExtended : GeometryBase, IGeoExtended
    {
        protected GeometryExtended() : base()
        {
            _geom4326 = new Lazy<IGeometry>(() => GetGeometry(() => Geom_4326));
            _geom3857 = new Lazy<IGeometry>(() => GetGeometry(() => Geom_3857));
        }

        private readonly Lazy<IGeometry> _geom4326;
        private readonly Lazy<IGeometry> _geom3857;

        [Column("geom_4326")]
        public virtual string Geom_4326 { get; set; }

        [Column("geom_3857")]
        public virtual string Geom_3857 { get; set; }


        public virtual IGeometry Geometry_4326
        {
            get { return _geom4326.Value; }
        }

        public virtual IGeometry Geometry_3857
        {
            get { return _geom3857.Value; }
        }
    }
}