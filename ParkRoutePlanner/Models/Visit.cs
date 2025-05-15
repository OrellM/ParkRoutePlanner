using System;
using System.Collections.Generic;

namespace ParkRoutePlanner.Models;

public partial class Visit
{
    public int VisitId { get; set; }

    public int? VisitorId { get; set; }

    public DateOnly? VisitDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public int? MinAge { get; set; }

    public int? MaxAge { get; set; }

    public int? MinHeightCm { get; set; }

    public virtual ICollection<VisitStation> VisitStations { get; set; } = new List<VisitStation>();

    public virtual Visitor? Visitor { get; set; }

    public virtual ICollection<RideCategory> Categories { get; set; } = new List<RideCategory>();

    public virtual ICollection<Ride> Rides { get; set; } = new List<Ride>();
}
