using System.Data.Entity.Spatial;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public interface ISection
    {
        decimal start { get; set; }
        decimal stop { get; set; }
    }

    public interface IGeo
    {
//        DbGeometry DbGeometry2 { get; }
        string Geom { get; set; }
        GeoAPI.Geometries.IGeometry OriginalGeometry { get; }
    }

    public interface IGeoExtended : IGeo
    {
        string Geom_4326 { get; set; }
        GeoAPI.Geometries.IGeometry Geometry_4326 { get; }
//        string Geom_3857 { get; set; }
//        GeoAPI.Geometries.IGeometry Geometry_3857 { get; }
    }
}