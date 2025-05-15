using System;
using System.Collections.Generic;

namespace ParkRoutePlanner.Models;

public partial class Visitor
{
    public int VisitorId { get; set; }

    public string VisitorName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
