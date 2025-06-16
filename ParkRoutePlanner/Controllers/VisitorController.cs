using Microsoft.AspNetCore.Mvc;
using ParkRoutePlanner.Models;

namespace ParkRoutePlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorController : ControllerBase
    {
        [HttpPost]
        public IActionResult PostVisitor([FromBody] VisitorModel visitor)
        {
            Console.WriteLine($"Age: {visitor.Age}, Height: {visitor.Height}");
            Console.WriteLine("Preferred Categories: " + string.Join(", ", visitor.PreferredCategories));
            Console.WriteLine("Visit Time: " + visitor.VisitStartTime + " to " + visitor.VisitEndTime);
            Console.WriteLine("Preferred Attractions: " + string.Join(", ", visitor.PreferredAttractions));

            ParkRoutePlanner.openingTime = TimeOnly.Parse(visitor.VisitStartTime);
            ParkRoutePlanner.closingTime = TimeOnly.Parse(visitor.VisitEndTime);

            return Ok(new { message = "Visitor saved successfully" });
        }
    }
}
