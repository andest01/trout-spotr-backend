using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TroutDash.DatabaseImporter.Convention;
using TroutStreamMangler.MN;
using TroutStreamMangler.US;

namespace TroutStreamMangler.Test
{
    [TestClass]
    public class RegionExportShapefileTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var regionDirectory = new DirectoryInfo("Regions");
            List<RegionModel> regionModels = regionDirectory.EnumerateFiles("*.csv", SearchOption.TopDirectoryOnly)
                .Select(RegionsBuilder.GetRegionModel)
                .ToList();


        }
    }

    public static class RegionShapefileExporter
    {
        // pgsql2shp -f <path to output shapefile> -h <hostname> -u <username> -P <password> databasename "<query>"
        public static void ExportRegionsToDatabase(IDatabaseConnection databaseConnection)
        {
            
        }
    }
}
