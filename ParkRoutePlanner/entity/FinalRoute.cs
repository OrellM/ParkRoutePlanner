using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkRoutePlanner.entity
{
    public class FinalRoute
    {
        public int Time { get; set; }

        //public string[] RidesRoute { get; set; }
        public List<RouteStop> RidesRoute { get; set; }

    }
}
