using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TroutDash.EntityFramework.Models;

namespace TroutDash.Export.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var exporter = new JsonExporter(new TroutDashPrototypeContext());
            var results = exporter.GetStreamDetails().ToList();

            var publicLand = results.Where(s => s.PalsLength > 0).ToList();
            var restrictions = results.Where(s => s.RestrictionsLength > 0).ToList();
            var lakes = results.Where(s => s.LakesLength > 0).ToList();
            
        }
    }
}
