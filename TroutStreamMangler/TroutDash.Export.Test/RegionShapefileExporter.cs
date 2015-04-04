using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TroutDash.DatabaseImporter.Convention;
using TroutDash.EntityFramework.Models;

namespace TroutDash.Export.Test
{
    [TestClass]
    public class RegionShapefileExporter
    {
        [TestMethod]
        public void TestMethod1()
        {
            var context = new TroutDashPrototypeContext();
            var shapes = new[]
            {
                "lake",
                "county",
                "stream",
                "trout_stream_section",
                "publicly_accessible_land",
                "restriction_route"
            };
            var targetDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent;
            var dbConnection = new DatabaseConnection("TroutDash", "localhost", "postgres");
            var t = new RegionToShapefileExporter(context, targetDirectory, shapes, dbConnection, new JsonExporter(context));
            t.Export();
        }
    }
}
