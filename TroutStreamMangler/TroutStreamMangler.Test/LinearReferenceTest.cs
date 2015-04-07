

using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using TroutStreamMangler.MN.Models;

namespace TroutStreamMangler.Test
{
    [TestClass]
    public class LinearReferenceTest
    {


        [TestMethod]
        public void TestMethod1()
        {
            var context = new MinnesotaShapeDataContext();
            var beaverRiver = context.StreamRoute.Single(i => i.kittle_nbr == "S-035");
            var troutSections = context.trout_streams_minnesota.Where(i => i.kittle_nbr == "S-035").ToList();
            var easements = context.mndnr_fisheries_acquisition.ToList();

            var intersections = easements.First(w => beaverRiver.OriginalGeometry.Intersects(w.OriginalGeometry)).OriginalGeometry;

            var streamGeometry = beaverRiver.OriginalGeometry as MultiLineString;

            Stuff(streamGeometry, troutSections.Select(i => i.OriginalGeometry).Cast<MultiLineString>().ToList(), intersections as MultiPolygon);
        }

        private void Stuff(MultiLineString multilinestring, IEnumerable<MultiLineString> troutStreamSections, MultiPolygon pal)
        {
            IGeometry[] t = multilinestring.Geometries;
            var stream = t.First();
            var primaryLength = stream.Length;
            var lil = new LocationIndexedLine(stream);
            var lil2 = new LengthIndexedLine(stream);
            var result = stream.Intersection(pal) as MultiLineString;
            var geoms = result.Geometries;
            foreach (var part in geoms.Select(i => i as LineString))
            {
                var subParts = lil.IndicesOf(part);
                var count = subParts.Length;
                foreach (var subPart in subParts)
                {
                    var fraction = subPart.SegmentFraction;
                    var noIdea = subPart.GetSegmentLength(part);
                    var otherNoIdea = subPart.GetSegmentLength(stream);

                }

                var subParts2 = lil2.IndicesOf(part);
                var count2 = subParts2.Length;
                foreach (double subPart in subParts2)
                {
                    
//                    var fraction = subPart.
//                    var noIdea = subPart.GetSegmentLength(part);
//                    var otherNoIdea = subPart.GetSegmentLength(stream);

                }
            }
        }
    }
}
