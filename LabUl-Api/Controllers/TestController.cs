using Microsoft.AspNetCore.Mvc;

namespace LabUlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("check")]
        public IActionResult Check()
        {
            var paths = new
            {
                CurrentDirectory = Directory.GetCurrentDirectory(),
                WwwRootExists = Directory.Exists("wwwroot"),
                ImagesPathExists = Directory.Exists("wwwroot/images")
            };

            return Ok(paths);
        }
    }
}