using System;
using System.Diagnostics;

namespace TroutStreamMangler.MN
{
    public class RegionNameModel
    {
        public string FileName { get; set; }
        public string FullName { get; set; }
    }

    [DebuggerDisplay("{name}: {FIPS}")]
    public class RegionCountyModel
    {
        public int gid { get; set; }
        public string statefp { get; set; }
        public string countyfp { get; set; }
        public string name { get; set; }

        public string FIPS
        {
            get
            {
                var tempcountyFp = "000" + countyfp ;
                tempcountyFp = tempcountyFp.Substring(tempcountyFp.Length - 3, 3);
                if (String.IsNullOrWhiteSpace(statefp) || String.IsNullOrWhiteSpace(tempcountyFp))
                {
                    return String.Empty;
                }

                return statefp + tempcountyFp;
            }
        }
    }
}