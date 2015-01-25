using System.Collections.Generic;
using GeoAPI.Geometries;

namespace TroudDash.GIS
{
    public interface ILinearReference
    {
        IEnumerable<double[]> GetIntersections(IMultiLineString linestring, IGeometry multipolygon);
        IEnumerable<double[]> GetIntersections(ILineString linestring, IGeometry multipolygon);
        IEnumerable<double> GetIntersectionOfLine(ILineString primaryLine, ILineString subline);
    }
}