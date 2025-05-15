using System;
using System.Collections.Generic;

namespace ParkRoutePlanner.Models;

public partial class Ride
{
    public int RideId { get; set; }

    public string RideName { get; set; } = null!;

    public int? Capacity { get; set; }

    public string? Location { get; set; }

    public int? AvgDurationMinutes { get; set; }

    public int? MinAge { get; set; }

    public int? MaxAge { get; set; }

    public int? MinHeightCm { get; set; }

    public int? AvgWaitTimeMinutes { get; set; }

    public string? Category { get; set; }

    public int? PopularityRating { get; set; }

    public string? OperatingDays { get; set; }

    public virtual ICollection<RideDistance> RideDistanceFromRides { get; set; } = new List<RideDistance>();

    public virtual ICollection<RideDistance> RideDistanceToRides { get; set; } = new List<RideDistance>();

    public virtual ICollection<VisitStation> VisitStations { get; set; } = new List<VisitStation>();

    public virtual ICollection<RideCategory> Categories { get; set; } = new List<RideCategory>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
