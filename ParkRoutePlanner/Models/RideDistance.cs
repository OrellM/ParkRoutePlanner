using System;
using System.Collections.Generic;

namespace ParkRoutePlanner.Models;

public partial class RideDistance
{
    public int FromRideId { get; set; }

    public int ToRideId { get; set; }

    public int? DistanceMeters { get; set; }

    public virtual Ride FromRide { get; set; } = null!;

    public virtual Ride ToRide { get; set; } = null!;
}
