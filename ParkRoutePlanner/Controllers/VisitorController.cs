
using Microsoft.AspNetCore.Mvc;
using ParkRoutePlanner.Models;
using Microsoft.Extensions.Configuration;

namespace ParkRoutePlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorController : ControllerBase
    {
        private readonly IConfiguration _config;

        public VisitorController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult PostVisitor([FromBody] VisitorModel visitor)
        {
            Console.WriteLine($"Age: {visitor.Age}, Height: {visitor.Height}");
            Console.WriteLine("Preferred Categories: " + string.Join(", ", visitor.PreferredCategories));
            Console.WriteLine("Visit Time: " + visitor.VisitStartTime + " to " + visitor.VisitEndTime);
            Console.WriteLine("Preferred Attractions: " + string.Join(", ", visitor.PreferredAttractions));

            // ?? יצירת מתכנן עם קונפיגורציה
            var planner = new ParkRoutePlanner(_config);

            // ? הגדרת שעות ביקור
            planner.OpeningTime = TimeOnly.Parse(visitor.VisitStartTime);
            planner.ClosingTime = TimeOnly.Parse(visitor.VisitEndTime);

            // ?? החזרה ללקוח
            return Ok(new { message = "Visitor saved successfully" });
        }
    }
}
