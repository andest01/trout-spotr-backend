using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace TroutStreamMangler.MN
{
    public static class RegionsBuilder
    {
        public static IEnumerable<T> GetCsvModel<T>(string fileName)
        {
            using (var writer = File.OpenText(fileName))
            {
                var csv = new CsvReader(writer);
                while (csv.Read())
                {
                    var record = csv.GetRecord<T>();

                    yield return record;
                }

            }
        }

        public static IEnumerable<RegionCountyModel> GetRegionCountyModels(string fileName)
        {
            return GetCsvModel<RegionCountyModel>(fileName);
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
        }

        public static RegionModel GetRegionModel(FileInfo regionFileName)
        {
            var rm = new RegionModel();
            rm.regionName = Path.GetFileNameWithoutExtension(regionFileName.FullName);
            rm.Counties = GetRegionCountyModels(regionFileName.FullName);
            return rm;
        }


    }
}