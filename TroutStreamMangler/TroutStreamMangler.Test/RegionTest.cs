using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TroutStreamMangler.MN;

namespace TroutStreamMangler.Test
{
    [TestClass]
    public class RegionTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var regionDirectory = new DirectoryInfo("Regions");
            var files = regionDirectory.EnumerateFiles("*.csv", SearchOption.TopDirectoryOnly).Select(RegionsBuilder.GetRegionModel).ToList();

        }
    }


//    public static class RegionsBuilder
//    {
//        public static IEnumerable<RegionCountyModel> GetRegionCountyModels(string fileName)
//        {
//            using (var writer = File.OpenText(fileName))
//            {
//                var csv = new CsvReader(writer);
//                while (csv.Read())
//                {
//                    var record = csv.GetRecord<RegionCountyModel>();
//
//                    yield return record;
//                }
//
//            }
//        }
//
//        public static RegionModel GetRegionModel(FileInfo regionFileName)
//        {
//            var rm = new RegionModel();
//            rm.regionName = regionFileName.Name;
//            rm.Counties = GetRegionCountyModels(regionFileName.FullName);
//            return rm;
//        }


    }

//    [DebuggerDisplay("{name}: {FIPS}")]
//    public class RegionCountyModel
//    {
//        public int gid { get; set; }
//        public string statefp { get; set; }
//        public string countyfp { get; set; }
//        public string name { get; set; }
//
//        public string FIPS
//        {
//            get
//            {
//                if (String.IsNullOrWhiteSpace(statefp) || String.IsNullOrWhiteSpace(countyfp))
//                {
//                    return String.Empty;
//                }
//
//                return statefp + countyfp;
//            }
//        }
//    }
//
//    [DebuggerDisplay("{regionName}, counties: {CountyCount}")]
//    public class RegionModel
//    {
//        public int id { get; set; }
//        public IEnumerable<RegionCountyModel> Counties { get; set; }
//
//        public int CountyCount
//        {
//            get
//            {
//                if (Counties == null)
//                {
//                    return 0;
//                }
//
//                return Counties.Count();
//            }
//        }
//        public string regionName { get; set; }
//    }

