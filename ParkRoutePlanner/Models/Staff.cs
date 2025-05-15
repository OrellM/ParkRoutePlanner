using System;
using System.Collections.Generic;

namespace ParkRoutePlanner.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string StaffName { get; set; } = null!;

    public string Password { get; set; } = null!;
}
