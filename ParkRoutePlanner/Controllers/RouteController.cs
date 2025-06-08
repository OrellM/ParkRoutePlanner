using Microsoft.AspNetCore.Mvc;
using ParkRoutePlanner.Models; // אם המחלקה שלך נמצאת ב־Models

namespace ParkRoutePlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RouteController : ControllerBase
{
    [HttpGet("optimal")]
    public IActionResult GetOptimalPath()
    {
        // במקום זה תכתבי את הקריאה לפונקציה שלך שמחזירה את הנתיב
         // נניח זו הפונקציה שלך

        return Ok();
    }
}
