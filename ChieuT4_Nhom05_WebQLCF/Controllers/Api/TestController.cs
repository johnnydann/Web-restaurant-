using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChieuT4_Nhom05_WebQLCF.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public TestController() { 
        }
        [HttpGet("test1")]
        [Authorize]
        public IActionResult Index1()
        {
            return Ok(new { status = true, message = "" });
        }
    }
}
