    using ChieuT4_Nhom05_WebQLCF.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    namespace ChieuT4_Nhom05_WebQLCF.Controllers.Api
    {
        [Route("api/[controller]")]
        [ApiController]
        [Authorize(Roles = SD.Role_Admin)]

        public class AdminApiController : ControllerBase
        {
            private readonly UserManager<ApplicationUser> _userManager;
            public AdminApiController(UserManager<ApplicationUser> userManager)
            {
                _userManager = userManager;
            }

            [HttpGet("getall", Name = "GetAllUser")]
            public async Task<IActionResult> GetUsers()
            {
                var listUsers = await _userManager.Users.ToListAsync();
                return Ok(listUsers); // Return the list of users in JSON format
            }

            [HttpPost("ban/{userId}")]
            public async Task<IActionResult> BanUser(string userId)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                return Ok(new { message = $"User {user.Email} has been banned." });
            }

            [HttpPost("unban/{userId}")]
            public async Task<IActionResult> UnbanUser(string userId)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                await _userManager.SetLockoutEndDateAsync(user, null);
                return Ok(new { message = $"User {user.Email} has been unbanned." });
            }
        }
    }
