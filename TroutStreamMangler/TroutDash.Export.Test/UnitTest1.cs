using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TroutDash.EntityFramework.Models;

namespace TroutDash.Export.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod2()
        {
            var exporter = new JsonExporter(new TroutDashPrototypeContext());
            var results = exporter.GetRegionDetails().ToList();
            var southEast = results.Where(r => r.RegionName.IndexOf("south", StringComparison.OrdinalIgnoreCase) >= 0);
            string output = JsonConvert.SerializeObject(southEast);
            
        }

        [TestMethod]
        public void TestMapList()
        {
            var exporter = new JsonExporter(new TroutDashPrototypeContext());
            var results = exporter.GetStreamDetails().ToList();
            string output = JsonConvert.SerializeObject(results);
            var summary = JsonConvert.DeserializeObject<List<StreamSummary>>(output);

            var things = summary.Where(i => i.TroutStreamsLength > 0).ToList();

            using (var writer = File.CreateText("TestMapList.csv"))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(summary);
            }
//            string output = JsonConvert.SerializeObject(southEast);

        }

        [TestMethod]
        public void TestMethod1()
        {
            var exporter = new JsonExporter(new TroutDashPrototypeContext());
            var results = exporter.GetStreamDetails().ToList();

            var publicLand = results.Where(s => s.PalsLength > 0).ToList();
            var restrictions = results.Where(s => s.RestrictionsLength > 0).ToList();
            var lakes = results.Where(s => s.LakesLength > 0).ToList();
            string output = JsonConvert.SerializeObject(results);
        }

        [TestMethod]
        public void CurrentLength()
        {
            var exporter = new JsonExporter(new TroutDashPrototypeContext());
            var results = exporter.GetStreamDetails().ToList();

            var totalMilesOfStreams = results.Sum(r => r.TroutStreamsLength);
            var totalMilesOfPublicLand = results.Sum(r => r.PalsLength);
        }

        [TestMethod]
        public void CreateEmptySpeciesCsv()
        {
            var context = new TroutDashPrototypeContext();
            var mnStreams = context.streams.Where(i => i.state == "Minnesota").ToList();

            var rows = mnStreams.Select(s =>
            {
                var f = new Func<bool, bool?>((b) =>
                {
                    if (!b)
                    {
                        return null;
                    }

                    return true;
                });

                return new StreamSpeciesCsv()
                {
                    Name = s.name,
                    Id = s.source,
                    Counties = string.Join(",", s.counties.Select(c => c.name)),
                    IsBrook = f(s.has_brook_trout),
                    IsBrown = f(s.has_brown_trout),
                    IsRainbow = f(s.has_rainbow_trout),
                    IsBrookStocked = f(s.is_brook_trout_stocked),
                    IsBrownStocked = f(s.is_brown_trout_stocked),
                    IsRainbowStocked = f(s.is_rainbow_trout_stocked),
                    Status = s.status_mes
                };
            }).ToList();

            using (var writer = File.CreateText("StreamSpeciesBlank.csv"))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(rows);
            }
            



        }

        [TestMethod]
        public void CreateNewLandCsv()
        {
            var exporter = new JsonExporter(new TroutDashPrototypeContext());
            var results = exporter.GetStreamDetails().ToList();

            var rows = results.Select(s =>
            {


                return new StreamNewLandCsv()
                {
                    Name = s.Name,
                    Id = s.Id,
                    NewLand = s.PalsLength,
                    latitude = s.CentroidLatitude,
                    longitude = s.CentroidLongitude
                };
            }).ToList();

            using (var writer = File.CreateText("StreamLength2.csv"))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(rows);
            }




        }
    }

    
}
