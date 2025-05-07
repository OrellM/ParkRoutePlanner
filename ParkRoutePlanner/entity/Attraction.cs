using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkRoutePlanner.entity
{
    public class Attraction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public int RideDuration { get; set; } // משך זמן המתקן
        //public int[] FutureLoad { get; set; } // אומדן עומס עתידי לפי שעה
    }
}
