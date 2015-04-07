using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroutStreamMangler.MN
{
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
