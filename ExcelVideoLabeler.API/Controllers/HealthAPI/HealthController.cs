using Microsoft.AspNetCore.Mvc;

namespace ExcelVideoLabler.API.Controllers.HealthAPI
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HealthController : ControllerBase
    {
        [HttpGet()]
        public IActionResult Ping()
        {
            return Ok(new { status = "OK", timestamp = DateTime.UtcNow });
        }
    }
}