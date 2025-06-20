using System;
using System.Collections.Generic;

namespace ParkRoutePlanner.Models;
public class Load
{
    public int Id { get; set; }
    public string Attraction { get; set; }
    public int Visitors { get; set; }
    public DateTime Timestamp { get; set; }
}
