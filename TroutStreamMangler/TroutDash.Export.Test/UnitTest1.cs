using System;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN.Models;

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
            string output = JsonConvert.SerializeObject(results);
            
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
    }

    
}
