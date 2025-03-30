using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ChieuT4_Nhom05_WebQLCF.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgoraApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AgoraApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("getAppId")]
        public IActionResult GetAppId()
        {
            var appId = _configuration["Agora:APP_ID"];  // Đọc appId từ appsettings.json

            // Kiểm tra nếu appId không hợp lệ (trong trường hợp chưa được cấu hình)
            if (string.IsNullOrEmpty(appId))
            {
                return BadRequest("AppId is not configured properly.");
            }

            return Ok(new { appId });
        }
    }
}
