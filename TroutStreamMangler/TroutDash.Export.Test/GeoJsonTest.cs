using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GeoJSON.Net.Feature;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TroutDash.EntityFramework.Models;

namespace TroutDash.Export.Test
{
    [TestClass]
    public class GeoJsonTest
    {
        [TestMethod]
        public void GetAlphabetDistribution()
        {
            var context = new TroutDashPrototypeContext();
            var jsonExporter = new JsonExporter(context);
            var streamDetails = jsonExporter.GetStreamDetails();

            var inputNames =
                streamDetails.Select(s => s.Name)
                    .Concat(streamDetails.SelectMany(x => x.LocalNames).Distinct())
                    .ToList();

            var alphabet = CreateAlphabet().ToArray();
            var sb = new StringBuilder();
            var alphabetString = new string(alphabet);
            var firstRow = "secondLetter\ttotalHitsSingleCharacter\t";
            sb.Append(firstRow);
            alphabet.ToList().ForEach(x => sb.Append("" + x + '\t'));
            sb.AppendLine();
            foreach (var firstLetter in alphabet)
            {
                var singleCharacter = "" + firstLetter;
                var totalHitsSingleCharacter = inputNames.Count(n => n.IndexOf(singleCharacter, StringComparison.OrdinalIgnoreCase) >= 0);
                
                sb.Append(String.Empty + firstLetter + '\t');
                sb.Append("" + totalHitsSingleCharacter + '\t');
                foreach (var secondLetter in alphabet)
                {
                    var soughtToken = "" + firstLetter + secondLetter;
                    var totalHits = inputNames.Count(n => n.IndexOf(soughtToken, StringComparison.OrdinalIgnoreCase) >= 0);
                    sb.Append("" + totalHits + '\t');
                }
                sb.AppendLine();
            }

            var results = sb.ToString();
        }

        [TestMethod]
        public void GetAlphabetDistribution2()
        {
            var context = new TroutDashPrototypeContext();
            var jsonExporter = new JsonExporter(context);
            var streamDetails = jsonExporter.GetStreamDetails();

            var inputNames =
                streamDetails.Select(s => s.Name)
                    .Concat(streamDetails.SelectMany(x => x.LocalNames).Distinct())
                    .ToList();

            var alphabet = CreateAlphabet().ToArray();
            var sb = new StringBuilder();
            var alphabetString = new string(alphabet);
            var firstRow = "firstLetter\tsecondLetter\tfirstLetterOnlyHits\ttwoLetterHits";
            sb.Append(firstRow);
//            alphabet.ToList().ForEach(x => sb.Append("" + x + '\t'));
            sb.AppendLine();
            foreach (var firstLetter in alphabet)
            {
                var singleCharacter = "" + firstLetter;
                var totalHitsSingleCharacter = inputNames.Count(n => n.IndexOf(singleCharacter, StringComparison.OrdinalIgnoreCase) >= 0);
                foreach (var secondLetter in alphabet)
                {
                    sb.Append(String.Empty + firstLetter + '\t');
                    sb.Append(String.Empty + secondLetter + '\t');
                    sb.Append(String.Empty + totalHitsSingleCharacter + '\t');
//                    sb.Append("" + totalHitsSingleCharacter + '\t');
                    var soughtToken = "" + firstLetter + secondLetter;
                    var totalHits = inputNames.Count(n => n.IndexOf(soughtToken, StringComparison.OrdinalIgnoreCase) >= 0);
                    sb.Append("" + totalHits + '\t');
                    sb.AppendLine();
                }
                
            }

            var results = sb.ToString();
        }

        private IEnumerable<char> CreateAlphabet()
        {
            for (char c = 'a'; c <= 'z'; c++)
            {
                yield return c;
            } 
        }
            
            
        [TestMethod]
        public void TestMethod1()
        {
            var context = new TroutDashPrototypeContext();
            var jsonExporter = new JsonExporter(context);
            var streamDetails = jsonExporter.GetStreamDetails();

            var detailsJson = JsonConvert.SerializeObject(streamDetails, Formatting.None);
            File.WriteAllText("streamDetails.json", detailsJson);

            var t = streamDetails.ToDictionary(i => i.Id, i => i);
            var json = File.ReadAllText("stream.geojson");
            var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json).Features;
            var oldCount = featureCollection.Count;
//            var orphans =
//                featureCollection.Where(f => t.Any(stream => stream.Key == Convert.ToInt32(f.Properties["gid"])))
//                    .ToList();
            featureCollection.RemoveAll(f => t.Any(stream => stream.Key == Convert.ToInt32(f.Properties["gid"])) == false);
            var newCount = featureCollection.Count;
            foreach (var f in featureCollection)
            {
                var gid = f.Properties["gid"];
                var featureId = Convert.ToInt32(gid);
                var jsonObject = t[featureId];
                var obj = JsonConvert.SerializeObject(jsonObject, Formatting.None);
                Dictionary<string, object> FD =
                    (from x in jsonObject.GetType().GetProperties() select x).ToDictionary(x => x.Name,
                        x =>
                            (x.GetGetMethod().Invoke(jsonObject, null) ?? ""));
//                f.Properties.Clear();
//                foreach (var entry in FD)
//                {
//                    f.Properties.Add(entry.Key, entry.Value);
//                }
            }

            var result = JsonConvert.SerializeObject(featureCollection, Formatting.None);
            File.WriteAllText("streamResult.json", json);
        }
    }
}