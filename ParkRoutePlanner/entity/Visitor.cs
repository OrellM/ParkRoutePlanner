using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkRoutePlanner.entity
{
    public class Visitor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int height { get; set; }
        public int TimeLimit { get; set; } // זמן שהייה בפארק
        public int[] Preferences { get; set; } // העדפות לפי מתקנים
    }
}
