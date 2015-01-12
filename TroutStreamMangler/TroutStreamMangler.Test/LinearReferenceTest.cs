using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Geometries;
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

            var easements = context.mndnr_fisheries_acquisition.ToList();

            var intersections = easements.First(w => beaverRiver.OriginalGeometry.Intersects(w.OriginalGeometry));

            var streamGeometry = beaverRiver.OriginalGeometry as MultiLineString;
            
        }
    }
}
