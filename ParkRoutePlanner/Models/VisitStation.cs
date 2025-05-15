using System;
using System.Collections.Generic;

namespace ParkRoutePlanner.Models;

public partial class VisitStation
{
    public int VisitId { get; set; }

    public int RideId { get; set; }

    public TimeOnly? EstimatedTime { get; set; }

    public int? StationNumber { get; set; }

    public bool? Visited { get; set; }

    public virtual Ride Ride { get; set; } = null!;

    public virtual Visit Visit { get; set; } = null!;
}
