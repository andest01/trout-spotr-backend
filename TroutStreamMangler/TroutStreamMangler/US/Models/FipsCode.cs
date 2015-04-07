using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroutStreamMangler.US.Models
{
    public class FipsCode
    {
        public string StateName { get; set; }
        public string StateAbbreviation { get; set; }
        public string CountyName { get; set; }
        public int CountyNumber { get; set; }
        public int StateNumber { get; set; }
    }
}