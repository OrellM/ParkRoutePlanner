using System;
using System.Collections.Generic;

namespace ParkRoutePlanner.Models;

public partial class RideCategory
{
    public int CategoryId { get; set; }

    public string? CategoryDescription { get; set; }

    public virtual ICollection<Ride> Rides { get; set; } = new List<Ride>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
