using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TroutStreamMangler.MN;

namespace TroutStreamMangler.US
{
    public class RoadTypeModel
    {
        // type, name, isFederal, stateId
        public string type { get; set; }
        public string name { get; set; }
        public bool isFederal { get; set; }
        public string stateId { get; set; }
    }


    [DebuggerDisplay("{regionName}, counties: {CountyCount}")]
    public class RegionModel
    {
        public int id { get; set; }
        public IEnumerable<RegionCountyModel> Counties { get; set; }

        public int CountyCount
        {
            get
            {
                if (Counties == null)
                {
                    return 0;
                }

                return Counties.Count();
            }
        }
        public string regionName { get; set; }
    }
}
