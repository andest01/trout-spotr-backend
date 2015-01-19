
using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Geometries;
using TroudDash.GIS;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN.Models;

namespace TroutStreamMangler.Test
{
    [TestClass]
    public class TroutStreamSectionsTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var context = new MinnesotaShapeDataContext();
            var snakeRiver = context.StreamRoute.Single(i => i.kittle_nbr == "H-001-092-021-007");
            var troutSections = context.trout_streams_minnesota.Where(i => i.kittle_nbr == "H-001-092-021-007").ToList();

            var sections = troutSections.SelectMany(i => CreateSection(i, snakeRiver)).ToList();
            
        }

        private IEnumerable<trout_stream_section> CreateSection(trout_streams_minnesota asdf, StreamRoute route)
        {
            var routeMultilineString = route.OriginalGeometry as IMultiLineString;
            var troutStreamSection = (asdf.OriginalGeometry as IMultiLineString);
            var desiredTroutStreamSection = (asdf.Geometry_3857 as IMultiLineString);
            var TroutStreamSection4236 = (asdf.Geometry_4326 as IMultiLineString);
            var numberOfGeometries = troutStreamSection.Geometries.Count();
            for (var i = 0; i < numberOfGeometries; i++)
            {
                var s = troutStreamSection.Geometries[i] as ILineString;
                var desiredGeometry = desiredTroutStreamSection.Geometries[i];
                var asdf4236 = TroutStreamSection4236.Geometries[i];

                var trout_section = new trout_stream_section();

                var centroid = asdf4236.Centroid;
                trout_section.centroid_latitude = Convert.ToDecimal(centroid.X);
                trout_section.centroid_longitude = Convert.ToDecimal(centroid.Y);
                trout_section.length_mi = Convert.ToDecimal(s.Length) / 1609.3440M;
                trout_section.public_length = 0;
                trout_section.section_name = asdf.kittle_nam ?? "Unnamed Stream";
                trout_section.source_id = asdf.kittle_nbr;

                var multilineString = new MultiLineString(new[] { s });

                var t = new LinearReference();
                var text = desiredGeometry.AsText();
                var bin = multilineString.AsBinary();

                trout_section.Geom = text;//asdf.Geom_3857;

                var result = t.GetIntersectionOfLine(routeMultilineString.Geometries.First() as ILineString, s).ToList();

                trout_section.start = (decimal)result[0];
                trout_section.stop = (decimal)result[1];

                yield return trout_section;

            }


            
        }
    }
}
