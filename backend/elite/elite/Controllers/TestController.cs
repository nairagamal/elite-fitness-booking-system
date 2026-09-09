using Microsoft.AspNetCore.Mvc;

namespace elite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow });
        }

        [HttpGet("hello/{name}")]
        [Produces("application/json")]
        public IActionResult Hello(string name)
        {
            return Ok(new { message = $"Hello, {name}!", timestamp = DateTime.UtcNow });
        }
    }
}